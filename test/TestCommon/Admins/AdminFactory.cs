using GymManagement.Domain.Admins;
using TestCommon.ConstantsTest;

namespace TestCommon.Admins;

public static class AdminFactory
{
    public static Admin CreateAdminWithNoSubscription(Guid? userId = null, Guid? subscriptionId = null, Guid? id = null)
    {
        return new Admin(
            userId ?? UserConstantsTest.Id,
            subscriptionId,
            id: id ?? AdminConstantsTest.Id);
    }
    
    public static Admin CreateAdmin(Guid? userId = null, Guid? subscriptionId = null, Guid? id = null)
    {
        return new Admin(
            userId ?? UserConstantsTest.Id,
            subscriptionId ?? SubscriptionConstantsTest.Id,
            id: id ?? AdminConstantsTest.Id);
    }
}