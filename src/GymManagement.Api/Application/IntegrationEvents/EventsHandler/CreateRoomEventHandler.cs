using GymManagement.EventBus.Messages;
using GymManagement.EventBus.Messages.IntegrationEvents.Events;
using MassTransit;
using MediatR;

namespace GymManagement.Api.Application.IntegrationEvents.EventsHandler;

public class CreateRoomEventHandler : BaseConsumer<CreateRoomEvent>
{
    private readonly IMediator _mediator;

    public CreateRoomEventHandler(ILogger<CreateRoomEventHandler> logger, IMediator mediator) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected override async Task ConsumeInternal(ConsumeContext<CreateRoomEvent> context)
    {
        
    }
}