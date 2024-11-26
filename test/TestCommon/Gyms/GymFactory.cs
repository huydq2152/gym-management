using GymManagement.Domain.Gyms;
using TestCommon.ConstantsTest;

namespace TestCommon.Gyms;

public static class GymFactory
{
    public static Gym CreateGym(
        string name = GymConstantsTest.Name,
        int maxRooms = SubscriptionConstantsTest.MaxRoomsFreeTier,
        Guid? id = null)
    {
        return new Gym(
            name,
            maxRooms,
            subscriptionId: SubscriptionConstantsTest.Id,
            id: id ?? GymConstantsTest.Id);
    }
}