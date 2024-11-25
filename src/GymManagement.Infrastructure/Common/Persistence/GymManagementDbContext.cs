using System.Reflection;
using GymManagement.Application.Common.Interfaces;
using GymManagement.Domain.Admins;
using GymManagement.Domain.Common;
using GymManagement.Domain.Gyms;
using GymManagement.Domain.Subscriptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Common.Persistence;

public class GymManagementDbContext : DbContext, IUnitOfWork
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublisher _publisher;

    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<Admin> Admins { get; set; } = null!;
    public DbSet<Gym> Gyms { get; set; } = null!;

    public GymManagementDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor,
        IPublisher publisher) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _publisher = publisher;
    }

    public async Task CommitChangesAsync(CancellationToken cancellationToken = default)
    {
        // get hold of all the domain events
        var domainEvents = ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity.PopDomainEvents())
            .SelectMany(x => x)
            .ToList();

        // store them in the http context for later if user is waiting online
        if (IsUserWaitingOnline())
        {
            AddDomainEventsToOfflineProcessingQueue(domainEvents);
        }
        else
        {
            await PublishDomainEvents(_publisher, domainEvents);
        }

        await SaveChangesAsync(cancellationToken);
    }

    private bool IsUserWaitingOnline() => _httpContextAccessor.HttpContext is not null;

    private static async Task PublishDomainEvents(IPublisher publisher, List<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }

    private void AddDomainEventsToOfflineProcessingQueue(List<IDomainEvent> domainEvents)
    {
        // fetch queue from http context or create a new queue if it doesn't exist
        var domainEventsQueue = _httpContextAccessor.HttpContext!.Items
            .TryGetValue("DomainEventsQueue", out var value) && value is Queue<IDomainEvent> existingDomainEventsQueue
            ? existingDomainEventsQueue
            : new Queue<IDomainEvent>();

        // add the domain events to the end of the queue
        foreach (var domainEvent in domainEvents)
        {
            domainEventsQueue.Enqueue(domainEvent);
        }

        // store the queue in the http context
        _httpContextAccessor.HttpContext.Items["DomainEventsQueue"] = domainEventsQueue;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}