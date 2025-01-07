using GymManagement.Domain.Common.CosmosDB;

namespace GymManagement.Domain.Rooms.CosmosDB;

public class CosmosDBRoom : CosmosDBEntity
{
    public string Name { get; } = null!;
    public Guid GymId { get; }
    public int MaxDailySessions { get; }
}