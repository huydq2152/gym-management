namespace GymManagement.Domain.Rooms;

public class RoomChange
{
    public Guid RoomId { get; set; }
    public bool CosmosDBUpdated { get; set; }
}