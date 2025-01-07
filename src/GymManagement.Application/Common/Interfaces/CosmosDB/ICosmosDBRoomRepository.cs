using GymManagement.Domain.Rooms.CosmosDB;

namespace GymManagement.Application.Common.Interfaces.CosmosDB;

public interface ICosmosDBRoomRepository: ICosmosDBRepository<CosmosDBRoom>
{
    Task<List<CosmosDBRoom>> GetRoomByGymIdAsync(Guid gymId);
}