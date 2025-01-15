using GymManagement.EventProcessor;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<RoomCosmosDbChangeFeedEventsProcessor>(); })
    .Build();

host.Run();