using GymManagement.Domain.Subscriptions;
using TestCommon.ConstantsTest;

namespace TestCommon.Subscriptions;

public static class SubscriptionFactory
{
    public static Subscription CreateSubscription(SubscriptionType? subscriptionType = null, Guid? adminId = null,
        Guid? id = null)
    {
        return new Subscription(subscriptionType ?? SubscriptionConstantsTest.DefaultSubscriptionType,
            adminId ?? AdminConstantsTest.Id,
            id ?? SubscriptionConstantsTest.Id);
    }
}