namespace GymManagement.EventBus.Messages.IntegrationEvents.Events;

public class RoomCosmosDbChangeFeedEvent
{
    public Guid RoomId { get; set; }
}