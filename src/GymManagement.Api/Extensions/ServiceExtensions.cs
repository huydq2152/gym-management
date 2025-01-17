using GymManagement.Api.Application.IntegrationEvents.EventsHandler;
using GymManagement.Api.Configurations;
using GymManagement.EventBus.Messages.IntegrationEvents.Events;
using GymManagement.Infrastructure.Extensions;
using MassTransit;

namespace GymManagement.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var eventBusSettings = configuration.GetSection(nameof(EventBusSettings))
            .Get<EventBusSettings>();
        services.AddSingleton(eventBusSettings ?? throw new ArgumentNullException(nameof(EventBusSettings)));

        return services;
    }

    public static void ConfigureMassTransit(this IServiceCollection services)
    {
        var eventBusSettings = services.GetOptions<EventBusSettings>(nameof(EventBusSettings));
        if (eventBusSettings == null || string.IsNullOrEmpty(eventBusSettings.HostAddress))
            throw new ArgumentNullException("EventBusSettings is not configured!");

        services.AddMassTransit(config =>
        {
            //config.AddServiceBusMessageScheduler();

            config.SetKebabCaseEndpointNameFormatter();

            config.AddConsumer<CreateRoomEventHandler>();
            config.AddConsumer<RoomCosmosDbChangeFeedEventHandler>();

            config.AddConfigureEndpointsCallback((_, cfg) =>
            {
                if (cfg is IServiceBusReceiveEndpointConfigurator sb)
                {
                    // Consider using this configuration can lead to infinite loop of dead lettering
                    sb.ConfigureDeadLetterQueueDeadLetterTransport();
                    sb.ConfigureDeadLetterQueueErrorTransport();
                }
            });

            config.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(eventBusSettings.HostAddress);

                //cfg.UseServiceBusMessageScheduler();
                
                // CreateRoomCosmosDBEvent will be published to a topic
                cfg.Send<CreateRoomCosmosDBEvent>(s =>
                {
                    // Note: If your bussiness logic is ecommerce, you can use UseSessionIdFormatter and set the OrderId as
                    // the session id for all message in an activity stream include OrderShipped, OrderSubmitted, OrderPaid, etc. 

                    s.UsePartitionKeyFormatter(c => c.Message.GymId.ToString("D"));
                });
                
                cfg.SubscriptionEndpoint<CreateRoomCosmosDBEvent>("create-room-event-handler", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Consumer<CreateRoomEventHandler>(context);
                });
                
                cfg.SubscriptionEndpoint<RoomCosmosDbChangeFeedEvent>("room-cosmosdb-change-feed-event-handler", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Consumer<RoomCosmosDbChangeFeedEventHandler>(context);
                });
            });
        });
    }
}