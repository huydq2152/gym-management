using GymManagement.Application.Rooms.Commands.CreateRoom.CosmosDB;
using GymManagement.EventBus.Messages;
using GymManagement.EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using MediatR;

namespace GymManagement.Api.Application.IntegrationEvents.EventsHandler;

public class CreateRoomEventHandler : BaseConsumer<CreateRoomCosmosDBEvent>
{
    private readonly ISender _mediator;

    public CreateRoomEventHandler(ILogger<CreateRoomEventHandler> logger, ISender mediator) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected override async Task ConsumeInternal(ConsumeContext<CreateRoomCosmosDBEvent> context)
    {
        var command = new CreateRoomCosmosDBCommand(context.Message.Id, context.Message.Name, context.Message.GymId,
            context.Message.MaxDailySessions);
        await _mediator.Send(command);
    }
}