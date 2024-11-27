namespace GymManagement.Application.SubcutaneousTests.Common;

[CollectionDefinition(CollectionName)]
public class MediatorFactoryCollection : ICollectionFixture<GymManagementApplicationFactory>
{
    public const string CollectionName = "MediatorFactoryCollection";
}