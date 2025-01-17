using GymManagement.EventProcessor.Configurations;

namespace GymManagement.EventProcessor.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var cosmosDbSettings = configuration.GetSection(nameof(CosmosDbSettings))
            .Get<CosmosDbSettings>();
        services.AddSingleton(cosmosDbSettings ?? throw new ArgumentNullException(nameof(CosmosDbSettings)));

        var eventBusSettings = configuration.GetSection(nameof(EventBusSettings))
            .Get<EventBusSettings>();
        services.AddSingleton(eventBusSettings ?? throw new ArgumentNullException(nameof(EventBusSettings)));

        return services;
    }
}