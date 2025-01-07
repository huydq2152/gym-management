using GymManagement.Domain.Common.CosmosDB;

namespace GymManagement.Domain.Rooms.CosmosDB;

public class CosmosDBRoom : CosmosDBEntity
{
    public string Name { get; set; } = null!;
    public Guid GymId { get; set; }
    public int MaxDailySessions { get; set; }
}