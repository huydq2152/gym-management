using GymManagement.EventProcessor.Extensions;
using GymManagement.EventProcessor.RoomCosmosDbChangeFeed;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => 
    {
        services
            .AddConfigurationSettings(context.Configuration)
            .AddHostedService<RoomCosmosDbChangeFeedEventsProcessor>();
    })
    .Build();

host.Run();