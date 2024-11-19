using GymManagement.Application.Common.Interfaces;
using GymManagement.Domain.Subscriptions;

namespace GymManagement.Infrastructure.Subscriptions.Persistence;

public class SubscriptionsRepository: ISubscriptionRepository
{
    private static readonly List<Subscription> _subscriptions = new();
    
    public Task AddSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        _subscriptions.Add(subscription);
        return Task.CompletedTask;
    }

    public Task<Subscription> GetSubscriptionAsync(Guid subscriptionId)
    {
        var result = _subscriptions.FirstOrDefault(subscription => subscription.Id == subscriptionId);
        if (result == null)
        {
            throw new Exception("Subscription not found");
        }
        return Task.FromResult(result);
    }
}