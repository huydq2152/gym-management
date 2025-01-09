namespace GymManagement.EventBus.Messages.IntegrationEvents.Interfaces;

public interface ICreateRoomEvent : IIntegrationEvent
{
    Guid Id { get; set; }
    string Name { get; set; }
    Guid GymId { get; set; }
    int MaxDailySessions { get; set; }
}