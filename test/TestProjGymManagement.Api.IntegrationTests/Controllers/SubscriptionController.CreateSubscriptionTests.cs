using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GymManagement.Contracts.Subscriptions;
using GymManagement.Infrastructure.Common.Persistence;
using TestCommon.Admins;
using TestCommon.ConstantsTest;
using TestProjGymManagement.Api.IntegrationTests.Common;

namespace TestProjGymManagement.Api.IntegrationTests.Controllers;

[Collection(GymManagementApiFactoryCollection.CollectionName)]
public class CreateSubscriptionTests
{
    private readonly HttpClient _client;
    private readonly GymManagementDbContext _dbContext;

    public CreateSubscriptionTests(GymManagementApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _dbContext = apiFactory.CreateDbContext();
    }
    
    [Theory]
    [MemberData(nameof(ListSubscriptionTypes))]
    public async Task CreateSubscription_WhenValidSubscription_ShouldCreateSubscription(SubscriptionType subscriptionType)
    {
        var admin = AdminFactory.CreateAdminWithNoSubscription();
        await _dbContext.Admins.AddAsync(admin);
        await _dbContext.SaveChangesAsync();
        
        // Arrange
        var createSubscriptionRequest = new CreateSubscriptionRequest(
            SubscriptionType: subscriptionType,
            AdminId: admin.Id);

        // Act
        var response = await _client.PostAsJsonAsync("Subscriptions", createSubscriptionRequest);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with status code {response.StatusCode}: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var subscriptionResponse = await response.Content.ReadFromJsonAsync<SubscriptionResponse>();
        subscriptionResponse.Should().NotBeNull();
        subscriptionResponse!.SubscriptionType.Should().Be(subscriptionType);

        response.Headers.Location!.PathAndQuery.Should().Be($"/Subscriptions/{subscriptionResponse.Id}");
        
        await _dbContext.Database.EnsureDeletedAsync();
    }

    public static TheoryData<SubscriptionType> ListSubscriptionTypes()
    {
        var subscriptionTypes = Enum.GetValues<SubscriptionType>().ToList();

        var theoryData = new TheoryData<SubscriptionType>();

        foreach (var subscriptionType in subscriptionTypes)
        {
            theoryData.Add(subscriptionType);
        }

        return theoryData;
    }
}