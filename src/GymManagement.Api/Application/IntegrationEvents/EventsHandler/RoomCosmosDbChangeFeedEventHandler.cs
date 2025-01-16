using GymManagement.Application.Rooms.Commands.CreateRoom.CosmosDB.RoomCosmosDbChangeFeed;
using GymManagement.EventBus.Messages;
using GymManagement.EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using MediatR;

namespace GymManagement.Api.Application.IntegrationEvents.EventsHandler;

public class RoomCosmosDbChangeFeedEventHandler : BaseConsumer<RoomCosmosDbChangeFeedEvent>
{
    private readonly ISender _mediator;

    public RoomCosmosDbChangeFeedEventHandler(ILogger<RoomCosmosDbChangeFeedEventHandler> logger, ISender mediator) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected override async Task ConsumeInternal(ConsumeContext<RoomCosmosDbChangeFeedEvent> context)
    {
        var command = new RoomCosmosDbChangeFeedCommand(context.Message.RoomId);
        await _mediator.Send(command);
    }
}