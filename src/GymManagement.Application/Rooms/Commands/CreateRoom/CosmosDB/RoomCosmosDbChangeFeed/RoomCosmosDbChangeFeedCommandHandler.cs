using ErrorOr;
using GymManagement.Application.Common.Interfaces;
using MediatR;

namespace GymManagement.Application.Rooms.Commands.CreateRoom.CosmosDB.RoomCosmosDbChangeFeed;

public class
    RoomCosmosDbChangeFeedCommandHandler : IRequestHandler<RoomCosmosDbChangeFeedCommand, ErrorOr<Success>>
{
    private readonly IRoomChangeRepository _roomChangeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RoomCosmosDbChangeFeedCommandHandler(IRoomChangeRepository roomChangeRepository, IUnitOfWork unitOfWork)
    {
        _roomChangeRepository = roomChangeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(RoomCosmosDbChangeFeedCommand request,
        CancellationToken cancellationToken)
    {
        var roomChange = await _roomChangeRepository.GetByRoomIdAsync(request.roomId);
        
        if (roomChange is null)
        {
            return Error.NotFound(description: "Room change not found");
        }
        
        roomChange.CosmosDBUpdated = true;
        
        await _roomChangeRepository.UpdateRoomChangeAsync(roomChange);
        await _unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}