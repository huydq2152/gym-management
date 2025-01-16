using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace GymManagement.EventProcessor.RoomCosmosDbChangeFeed;

/// <summary>
/// Background service to process change feed events from container Room in Cosmos DB and send them to Azure Service Bus.
/// Note: Can improve by using masstransit like API project to interact with the service bus.
/// </summary>
public class RoomCosmosDbChangeFeedEventsProcessor : BackgroundService
{
    private const string EventType = "mutationRoomCosmosDb";
    private readonly IConfiguration _configuration;
    private readonly Container _container;
    private readonly Container _leaseContainer;
    private readonly ILogger<RoomCosmosDbChangeFeedEventsProcessor> _logger;
    private readonly ServiceBusClient _sbClient;
    private readonly ServiceBusSender _topicSender;
    private ChangeFeedProcessor _cfp;

    public RoomCosmosDbChangeFeedEventsProcessor(ILogger<RoomCosmosDbChangeFeedEventsProcessor> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        _sbClient = new ServiceBusClient(_configuration.GetSection("ServiceBus")["ConnectionString"]);
        _topicSender = _sbClient.CreateSender(_configuration.GetSection("ServiceBus")["Topic:RoomCosmosDbChangeFeed"]);

        var cOpts = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            }
        };
        var containers = new List<(string, string)>
        {
            (_configuration.GetSection("CosmosDB")["DatabaseName"], _configuration.GetSection("CosmosDB")["Container:Room"])
        };
        var cosmosClient = CosmosClient.CreateAndInitializeAsync(_configuration.GetSection("CosmosDB")["EndpointUrl"],
            _configuration.GetSection("CosmosDB")["PrimaryKey"], containers, cOpts).Result;

        _container = cosmosClient.GetContainer(_configuration.GetSection("CosmosDB")["DatabaseName"],
            _configuration.GetSection("CosmosDB")["Container:Room"]);
        _leaseContainer = cosmosClient.GetDatabase(_configuration.GetSection("CosmosDB")["DatabaseName"])
            .CreateContainerIfNotExistsAsync(_configuration.GetSection("CosmosDB")["LeaseContainer"],
                "/id").Result;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(RoomCosmosDbChangeFeedEventsProcessor)} running at: {DateTimeOffset.UtcNow}.");
        _cfp = await StartChangeFeedProcessorAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Stopping {nameof(RoomCosmosDbChangeFeedEventsProcessor)} at: {DateTimeOffset.UtcNow}");
        await _cfp.StopAsync();
        await _topicSender.DisposeAsync();
        await _sbClient.DisposeAsync();
        _logger.LogInformation($"{nameof(RoomCosmosDbChangeFeedEventsProcessor)} stopped.");
    }

    private async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync()
    {
        var changeFeedProcessor = _container
            .GetChangeFeedProcessorBuilder<RoomCosmosDbChangeFeedDocument>(
                _configuration.GetSection("CosmosDB")["ProcessorName"],
                HandleChangesAsync)
            .WithInstanceName(Environment.MachineName)
            .WithLeaseContainer(_leaseContainer)
            .WithMaxItems(25)
            .WithStartTime(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .WithPollInterval(TimeSpan.FromSeconds(3))
            .Build();

        _logger.LogInformation("Starting Cosmos Change Feed Processor...");
        await changeFeedProcessor.StartAsync();
        _logger.LogInformation("Cosmos Change Feed Processor started.  Waiting for new messages to arrive.");
        return changeFeedProcessor;
    }

    private async Task HandleChangesAsync(IReadOnlyCollection<RoomCosmosDbChangeFeedDocument> changes,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received {changes.Count} document(s).");
        var eventsCount = 0;

        Dictionary<string, List<ServiceBusMessage>> partitionedMessages = new();

        foreach (var document in changes)
        {
            if (document.Type != EventType) continue;
            var json = JsonConvert.SerializeObject(document.Data);
            var sbMessage = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = "RoomCosmosDbChangeFeed",
                MessageId = document.Id,
                PartitionKey = document.PartitionKey,
                SessionId = document.PartitionKey
            };

            // Create message batch per partitionKey
            if (partitionedMessages.ContainsKey(document.PartitionKey))
            {
                partitionedMessages[sbMessage.PartitionKey].Add(sbMessage);
            }
            else
            {
                partitionedMessages[sbMessage.PartitionKey] = new List<ServiceBusMessage> { sbMessage };
            }

            eventsCount++;
        }

        if (partitionedMessages.Count > 0)
        {
            _logger.LogInformation(
                $"Processing {eventsCount} event(s) in {partitionedMessages.Count} partition(s).");

            // Loop over each partition
            foreach (var partition in partitionedMessages)
            {
                // Create batch for partition
                using var messageBatch =
                    await _topicSender.CreateMessageBatchAsync(cancellationToken);
                foreach (var msg in partition.Value)
                    if (!messageBatch.TryAddMessage(msg))
                        throw new Exception();

                _logger.LogInformation(
                    $"Sending {messageBatch.Count} event(s) to Service Bus. PartitionId: {partition.Key}");

                try
                {
                    await _topicSender.SendMessagesAsync(messageBatch, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    throw;
                }
            }
        }
        else
        {
            _logger.LogInformation("No event documents in change feed batch. Waiting for new messages to arrive.");
        }
    }
}