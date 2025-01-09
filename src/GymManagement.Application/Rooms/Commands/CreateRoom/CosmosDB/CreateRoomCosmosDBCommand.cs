using ErrorOr;
using GymManagement.Domain.Rooms.CosmosDB;
using MediatR;

namespace GymManagement.Application.Rooms.Commands.CreateRoom.CosmosDB;

public record CreateRoomCosmosDBCommand(Guid Id, string Name, Guid GymId, int MaxDailySessions) : IRequest<ErrorOr<CosmosDBRoom>>;