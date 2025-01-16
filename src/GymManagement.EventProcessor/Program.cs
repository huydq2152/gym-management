using GymManagement.EventProcessor;
using GymManagement.EventProcessor.RoomCosmosDbChangeFeed;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<RoomCosmosDbChangeFeedEventsProcessor>(); })
    .Build();

host.Run();