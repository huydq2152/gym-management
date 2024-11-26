using GymManagement.Application.Gyms.Commands.CreateGym;
using TestCommon.ConstantsTest;

namespace TestCommon.Gyms;

public static class GymCommandFactory
{
    public static CreateGymCommand CreateCreateGymCommand(
        string name = GymConstantsTest.Name,
        Guid? subscriptionId = null)
    {
        return new CreateGymCommand(
            Name: name,
            SubscriptionId: subscriptionId ?? SubscriptionConstantsTest.Id);
    }
}