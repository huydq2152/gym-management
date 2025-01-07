using GymManagement.Infrastructure.Common.Persistence.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;

namespace GymManagement.Infrastructure.Common.Persistence.CosmosDb
{
    public class CosmosDbContainer : ICosmosDbContainer
    {
        public Container Container { get; }

        public CosmosDbContainer(CosmosClient cosmosClient,
                                 string databaseName,
                                 string containerName)
        {
            Container = cosmosClient.GetContainer(databaseName, containerName);
        }
    }
}
