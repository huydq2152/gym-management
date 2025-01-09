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
        var settings = services.GetOptions<EventBusSettings>(nameof(EventBusSettings));
        if (settings == null || string.IsNullOrEmpty(settings.HostAddress) ||
            string.IsNullOrEmpty(settings.HostAddress))
            throw new ArgumentNullException("EventBusSettings is not configured!");

        var serviceBusConnection = new Uri(settings.HostAddress);
        //services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.UsingAzureServiceBus((_, cfg) =>
            {
                cfg.Host(serviceBusConnection);
                cfg.Send<CreateRoomCosmosDBEvent>(s => s.UseSessionIdFormatter(c => c.Message.Id.ToString("D")));
            });
        });
    }
}