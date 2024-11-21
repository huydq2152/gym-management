using Microsoft.Extensions.DependencyInjection;

namespace GymManagement.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection));
        });
    }
}