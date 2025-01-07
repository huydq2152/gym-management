using Microsoft.Azure.Cosmos;

namespace GymManagement.Infrastructure.Common.Persistence.CosmosDb.Interfaces
{
    public interface ICosmosDbContainer
    {
        /// <summary>
        ///     Instance of Azure Cosmos DB Container class
        /// </summary>
        Container Container { get; }
    }
}
