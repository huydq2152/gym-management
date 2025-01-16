using ErrorOr;
using GymManagement.Domain.Rooms.CosmosDB;
using MediatR;

namespace GymManagement.Application.Rooms.Commands.CreateRoom.CosmosDB.RoomCosmosDbChangeFeed;

public record RoomCosmosDbChangeFeedCommand(Guid roomId) : IRequest<ErrorOr<CosmosDBRoom>>;