using FluentAssertions;
using GymManagement.Application.SubcutaneousTests.Common;
using GymManagement.Domain.Subscriptions;
using GymManagement.Infrastructure.Common.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TestCommon.Admins;
using TestCommon.Gyms;
using TestCommon.Subscriptions;

namespace GymManagement.Application.SubcutaneousTests.Gyms.Commands;

[Collection(MediatorFactoryCollection.CollectionName)]
public class CreateGymTests
{
    private readonly MediatorFactory _mediatorFactoryProvider;
    private readonly IMediator _mediatorFactory;

    public CreateGymTests(MediatorFactory mediatorFactoryProvider)
    {
        _mediatorFactory = mediatorFactoryProvider.CreateMediator();
        _mediatorFactoryProvider = mediatorFactoryProvider;
    }

    [Fact]
    public async Task CreateGym_WhenValidCommand_ShouldCreateGym()
    {
        using var scope = _mediatorFactoryProvider.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymManagementDbContext>();
        var admin = AdminFactory.CreateAdminWithNoSubscription();
        await dbContext.Admins.AddAsync(admin);
        await dbContext.SaveChangesAsync();
        
        // Arrange
        var subscription = await CreateSubscription();

        // Create a valid CreateGymCommand
        var createGymCommand = GymCommandFactory.CreateCreateGymCommand(subscriptionId: subscription.Id);

        // Act
        var createGymResult = await _mediatorFactory.Send(createGymCommand);

        // Assert
        createGymResult.IsError.Should().BeFalse();
        createGymResult.Value.SubscriptionId.Should().Be(subscription.Id);

        var createdGym = await dbContext.Gyms.FindAsync(createGymResult.Value.Id);
        createdGym.Should().NotBeNull();
        createdGym!.Name.Should().Be(createGymCommand.Name);
        createdGym.SubscriptionId.Should().Be(subscription.Id);
        
        await dbContext.Database.EnsureDeletedAsync();
    }
    
    [Fact]
    public async Task CreateGym_WhenInputSubscriptionIdNotFound_ShouldReturnError()
    {
        // Create a CreateGymCommand with a non-existing subscriptionId
        var nonExistingSubscriptionId = Guid.NewGuid();
        var createGymCommand = GymCommandFactory.CreateCreateGymCommand(subscriptionId: nonExistingSubscriptionId);

        // Act
        var createGymResult = await _mediatorFactory.Send(createGymCommand);

        // Assert
        createGymResult.IsError.Should().BeTrue();
        createGymResult.FirstError.Description.Should().Be("Subscription not found");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(200)]
    public async Task CreateGym_WhenCommandContainsInvalidData_ShouldReturnValidationError(int gymNameLength)
    {
        // Arrange
        string gymName = new('a', gymNameLength);
        var createGymCommand = GymCommandFactory.CreateCreateGymCommand(name: gymName);

        // Act
        var result = await _mediatorFactory.Send(createGymCommand);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Name");
    }

    private async Task<Subscription> CreateSubscription()
    {
        //  1. Create a CreateSubscriptionCommand
        var createSubscriptionCommand = SubscriptionCommandFactory.CreateCreateSubscriptionCommand();

        //  2. Sending it to MediatR
        var result = await _mediatorFactory.Send(createSubscriptionCommand);

        //  3. Making sure it was created successfully
        result.IsError.Should().BeFalse();
        return result.Value;
    }
}