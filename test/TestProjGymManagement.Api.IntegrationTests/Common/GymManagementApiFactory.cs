using GymManagement.Api;
using GymManagement.Infrastructure.Common.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace TestProjGymManagement.Api.IntegrationTests.Common;

public class GymManagementApiFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.Remove(services.Single(s => s.ServiceType == typeof(DbContextOptions<GymManagementDbContext>)));

            services.AddDbContext<GymManagementDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
                options.ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning)); 
            }, ServiceLifetime.Singleton);
        });
    }
    
    public GymManagementDbContext CreateDbContext()
    {
        var serviceScope = Services.CreateScope();
        return serviceScope.ServiceProvider.GetRequiredService<GymManagementDbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public new Task DisposeAsync() => Task.CompletedTask;
}