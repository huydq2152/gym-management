using GymManagement.Api;
using GymManagement.Domain.Gyms;
using GymManagement.Infrastructure.Common.Persistence;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace GymManagement.Application.SubcutaneousTests.Common;

public class MediatorFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Mock DbContext
            var mockDbContext = new Mock<GymManagementDbContext>();

            // Mock DbSet<Gym>
            var mockGyms = new Mock<DbSet<Gym>>();
            mockDbContext.Setup(x => x.Gyms).Returns(mockGyms.Object);

            // Mock add method of DbSet<Gym>
            mockGyms.Setup(x => x.Add(It.IsAny<Gym>()))
                .Callback<Gym>(gym =>
                {
                    mockGyms.Object.Add(gym);
                });

            // Mock SaveChangesAsync method of DbContext
            services.RemoveAll<GymManagementDbContext>();
            services.AddScoped(sp => mockDbContext.Object);
        });
    }

    public IMediator CreateMediator()
    {
        var serviceScope = Services.CreateScope();
        return serviceScope.ServiceProvider.GetRequiredService<IMediator>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public new Task DisposeAsync() => Task.CompletedTask;
}