using GymManagement.Application.Common.Interfaces.CosmosDB;
using GymManagement.Domain.Admins.CosmosDB;
using GymManagement.Infrastructure.Common.Persistence.CosmosDb;
using GymManagement.Infrastructure.Common.Persistence.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;

namespace GymManagement.Infrastructure.Admins.Persistence.CosmosDB;

public class CosmosDBAdminRepository : CosmosDbRepository<CosmosDBAdmin>, ICosmosDBAdminRepository
{
    public CosmosDBAdminRepository(ICosmosDbContainerFactory cosmosDbContainerFactory) : base(cosmosDbContainerFactory)
    {
    }

    public override string ContainerName { get; } = "Admin";

    public override Guid GenerateId(CosmosDBAdmin entity)
    {
        return Guid.NewGuid();
    }

    public override PartitionKey ResolvePartitionKey(Guid entityId)
    {
        return new PartitionKey(entityId.ToString());
    }
}