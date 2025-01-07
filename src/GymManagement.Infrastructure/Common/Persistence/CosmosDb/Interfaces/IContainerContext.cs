using GymManagement.Domain.Common.CosmosDB;
using Microsoft.Azure.Cosmos;

namespace GymManagement.Infrastructure.Common.Persistence.CosmosDb.Interfaces
{
    /// <summary>
    ///  Defines the container level context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IContainerContext<T> where T : CosmosDBEntity
    {
        string ContainerName { get; }
        Guid GenerateId(T entity);
        PartitionKey ResolvePartitionKey(Guid entityId);
    }
}
