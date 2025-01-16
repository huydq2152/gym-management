using ErrorOr;
using MediatR;

namespace GymManagement.Application.Rooms.Commands.CreateRoom.CosmosDB.RoomCosmosDbChangeFeed;

public record RoomCosmosDbChangeFeedCommand(Guid roomId) : IRequest<ErrorOr<Success>>;