using Ardalis.Specification;

namespace GymManagement.Domain.Rooms.CosmosDB;

public sealed class RoomGetByGymIdSpecification: Specification<CosmosDBRoom>
{
    /// <summary>
    ///     Get all rooms in a specific gym by gymId
    /// </summary>
    /// <param name="gymId"></param>
    public RoomGetByGymIdSpecification(Guid gymId)
    {
        Query.Where(room =>
            room.GymId == gymId
        );
    }
}