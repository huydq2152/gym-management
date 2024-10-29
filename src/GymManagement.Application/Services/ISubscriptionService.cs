namespace GymManagement.Application.Services;

public interface ISubscriptionService
{
    Guid CreateSubscription(string subscriptionType, Guid adminId);
}