using GymManagement.EventBus.Messages.IntegrationEvents.Interfaces;

namespace GymManagement.EventBus.Messages.IntegrationEvents.Events;

public record CreateRoomCosmosDBEvent : IntegrationBaseEvent, ICreateRoomEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid GymId { get; set; }
    public int MaxDailySessions { get; set; }
}