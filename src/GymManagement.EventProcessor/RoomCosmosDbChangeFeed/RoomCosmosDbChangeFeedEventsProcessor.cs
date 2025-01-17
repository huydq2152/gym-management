using Azure.Messaging.ServiceBus;
using GymManagement.EventProcessor.Configurations;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GymManagement.EventProcessor.RoomCosmosDbChangeFeed;

/// <summary>
/// Background service to process change feed events from container Room in Cosmos DB and send them to Azure Service Bus.
/// Note: Can improve by using masstransit like API project to interact with the service bus.
/// </summary>
public class RoomCosmosDbChangeFeedEventsProcessor : BackgroundService
{
    private const string EventType = "mutationRoomCosmosDb";
    private readonly EventBusSettings _eventBusSettings;
    private readonly CosmosDbSettings _cosmosDbSettings;
    private readonly Container _container;
    private readonly Container _leaseContainer;
    private readonly ILogger<RoomCosmosDbChangeFeedEventsProcessor> _logger;
    private readonly ServiceBusClient _sbClient;
    private readonly ServiceBusSender _topicSender;
    private ChangeFeedProcessor _cfp;

    public RoomCosmosDbChangeFeedEventsProcessor(ILogger<RoomCosmosDbChangeFeedEventsProcessor> logger,
        IOptions<EventBusSettings> eventBusSettings, IOptions<CosmosDbSettings> cosmosDbSettings)
    {
        _logger = logger;

        _eventBusSettings = eventBusSettings.Value;
        _sbClient = new ServiceBusClient(_eventBusSettings.HostAddress);
        _topicSender = _sbClient.CreateSender(_eventBusSettings.Topics.RoomCosmosDbChangeFeed.Name);

        var cosmosClient = BuildCosmosClient();
        _cosmosDbSettings = cosmosDbSettings.Value;
        InitializeContainersAsync(cosmosClient,
            _cosmosDbSettings.DatabaseName,
            _cosmosDbSettings.Containers.Room.Name,
            _cosmosDbSettings.Containers.Room.PartitionKey,
            _cosmosDbSettings.Containers.RoomLeases.Name,
            _cosmosDbSettings.Containers.RoomLeases.PartitionKey).Wait();
        _container = cosmosClient.GetContainer(_cosmosDbSettings.DatabaseName, _cosmosDbSettings.Containers.Room.Name);
        _leaseContainer =
            cosmosClient.GetContainer(_cosmosDbSettings.DatabaseName, _cosmosDbSettings.Containers.RoomLeases.Name);
    }

    private async Task InitializeContainersAsync(
        CosmosClient cosmosClient,
        string? databaseName, string? sourceContainerName, string? sourceContainerPartitionKey,
        string? leaseContainerName, string? leaseContainerPartitionKey)
    {
        if (string.IsNullOrEmpty(databaseName)
            || string.IsNullOrEmpty(sourceContainerName)
            || string.IsNullOrEmpty(sourceContainerPartitionKey)
            || string.IsNullOrEmpty(leaseContainerName)
            || string.IsNullOrEmpty(leaseContainerPartitionKey))
        {
            throw new ArgumentNullException(
                "'DatabaseName', 'Containers:Room:Name', 'Containers:Room:PartitionKey', 'Containers:RoomLeases:Name' and 'Containers:RoomLeases:PartitionKey' settings are required. Verify your configuration.");
        }

        Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

        await database.CreateContainerIfNotExistsAsync(new ContainerProperties(sourceContainerName,
            sourceContainerPartitionKey));
        await database.CreateContainerIfNotExistsAsync(new ContainerProperties(leaseContainerName,
            leaseContainerPartitionKey));
    }

    private CosmosClient BuildCosmosClient()
    {
        if (string.IsNullOrEmpty(_cosmosDbSettings.EndpointUrl) ||
            string.IsNullOrEmpty(_cosmosDbSettings.PrimaryKey))
        {
            throw new ArgumentNullException("Missing 'EndpointUrl' or 'PrimaryKey' settings in configuration.");
        }

        return new CosmosClientBuilder(_cosmosDbSettings.EndpointUrl, _cosmosDbSettings.PrimaryKey)
            .WithSerializerOptions(new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            })
            .Build();
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
                _cosmosDbSettings.ProcessorName,
                HandleChangesAsync)
            .WithLeaseAcquireNotification(OnLeaseAcquiredAsync)
            .WithLeaseReleaseNotification(OnLeaseReleaseAsync)
            .WithErrorNotification(OnErrorAsync)
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

        Task OnErrorAsync(string leaseToken, Exception exception)
        {
            if (exception is ChangeFeedProcessorUserException userException)
            {
                Console.WriteLine(
                    $"Lease {leaseToken} processing failed with unhandled exception from user delegate {userException.InnerException}");
            }
            else
            {
                Console.WriteLine($"Lease {leaseToken} failed with {exception}");
            }

            return Task.CompletedTask;
        }

        Task OnLeaseReleaseAsync(string leaseToken)
        {
            Console.WriteLine($"Lease {leaseToken} is released and processing is stopped");
            return Task.CompletedTask;
        }

        Task OnLeaseAcquiredAsync(string leaseToken)
        {
            Console.WriteLine($"Lease {leaseToken} is acquired and will start processing");
            return Task.CompletedTask;
        }
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