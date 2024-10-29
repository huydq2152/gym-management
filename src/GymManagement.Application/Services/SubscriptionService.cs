namespace GymManagement.Application.Services;

public class SubscriptionService: ISubscriptionService
{
    public Guid CreateSubscription(string subscriptionType, Guid adminId)
    {
        return Guid.NewGuid();
    }
}