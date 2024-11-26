using ErrorOr;
using FluentAssertions;
using GymManagement.Domain.Subscriptions;
using TestCommon.Gyms;
using TestCommon.Subscriptions;


namespace GymManagement.Domain.UnitTest;

public class SubscriptionTests
{
    [Fact]
    public void AddGym_WhenHaveMoreGymsThanAllowed_ShouldReturnError()
    {
        // Arrange
        // Create a subscription
        var subscription = SubscriptionFactory.CreateSubscription();
        
        // Create the maximum number of gyms + 1
        var gyms = Enumerable.Range(0, subscription.GetMaxGyms() + 1)
            .Select(_ => GymFactory.CreateGym())
            .ToList();
        
        // Act
        // Add gyms to the subscription
        var addGymResults = gyms.ConvertAll(subscription.AddGym);

        // Assert
        // Check that all gyms except the last one were added successfully
        var allButLastGymResults = addGymResults.GetRange(0, addGymResults.Count - 1); 
        allButLastGymResults.Should().AllSatisfy(addGymResult => addGymResult.Value.Should().Be(Result.Success));

        // Check that the last gym was not added successfully
        var lastAddGymResult = addGymResults.Last();
        lastAddGymResult.IsError.Should().BeTrue();
        lastAddGymResult.FirstError.Should().Be(SubscriptionErrors.CannotHaveMoreGymsThanTheSubscriptionAllows);
    }
}