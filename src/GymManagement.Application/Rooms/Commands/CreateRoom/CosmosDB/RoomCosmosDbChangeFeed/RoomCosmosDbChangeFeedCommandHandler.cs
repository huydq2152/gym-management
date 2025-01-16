using ErrorOr;
using GymManagement.Domain.Rooms.CosmosDB;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GymManagement.Application.Rooms.Commands.CreateRoom.CosmosDB.RoomCosmosDbChangeFeed;

public class
    RoomCosmosDbChangeFeedCommandHandler : IRequestHandler<RoomCosmosDbChangeFeedCommand, ErrorOr<CosmosDBRoom>>
{
    private readonly ILogger<RoomCosmosDbChangeFeedCommandHandler> _logger;

    public RoomCosmosDbChangeFeedCommandHandler(ILogger<RoomCosmosDbChangeFeedCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<ErrorOr<CosmosDBRoom>> Handle(RoomCosmosDbChangeFeedCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "RoomCosmosDb updated: {roomId}, change the status of Room in sql server to cosmos synchronized",
            request.roomId);
        return new CosmosDBRoom
        {
            Id = request.roomId
        };
    }
}