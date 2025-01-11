using ErrorOr;
using GymManagement.Application.Common.Interfaces.CosmosDB;
using GymManagement.Domain.Rooms.CosmosDB;
using MediatR;

namespace GymManagement.Application.Rooms.Commands.CreateRoom.CosmosDB;

public class CreateRoomCosmosDBCommandHandler: IRequestHandler<CreateRoomCosmosDBCommand, ErrorOr<CosmosDBRoom>>
{
    private readonly ICosmosDBRoomRepository _cosmosDBRoomRepository;

    public CreateRoomCosmosDBCommandHandler(ICosmosDBRoomRepository cosmosDbRoomRepository)
    {
        _cosmosDBRoomRepository = cosmosDbRoomRepository;
    }

    public async Task<ErrorOr<CosmosDBRoom>> Handle(CreateRoomCosmosDBCommand request, CancellationToken cancellationToken)
    {
        var cosmosDBRoom = new CosmosDBRoom
        {
            Id = request.Id,
            Name = request.Name,
            GymId = request.GymId,
            MaxDailySessions = request.MaxDailySessions
        };
        await _cosmosDBRoomRepository.AddItemAsync(cosmosDBRoom, request.GymId);
        
        return cosmosDBRoom;
    }
}