using GymManagement.Application.Common.Interfaces.CosmosDB;
using GymManagement.Domain.Rooms.CosmosDB;
using GymManagement.Infrastructure.Common.Persistence.CosmosDb;
using GymManagement.Infrastructure.Common.Persistence.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Rooms.Persistence.CosmosDB;

public class CosmosDbRoomRepository : CosmosDbRepository<CosmosDBRoom>, ICosmosDBRoomRepository
{
    public CosmosDbRoomRepository(ICosmosDbContainerFactory cosmosDbContainerFactory) : base(cosmosDbContainerFactory)
    {
    }

    public override string ContainerName { get; } = "Room";

    public override Guid GenerateId(CosmosDBRoom entity)
    {
        return Guid.NewGuid();
    }

    public override PartitionKey ResolvePartitionKey(Guid entityId)
    {
        return new PartitionKey(entityId.ToString());
    }

    public async Task<List<CosmosDBRoom>> GetRoomByGymIdAsync(Guid gymId)
    {
        var getRoomByGymIdSpecification = new RoomGetByGymIdSpecification(gymId);
        return await ApplySpecification(getRoomByGymIdSpecification).ToListAsync();
    }
}