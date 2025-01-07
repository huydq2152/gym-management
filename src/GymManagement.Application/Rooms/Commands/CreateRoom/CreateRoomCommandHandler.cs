using ErrorOr;
using GymManagement.Application.Common.Interfaces;
using GymManagement.Application.Common.Interfaces.CosmosDB;
using GymManagement.Domain.Rooms;
using GymManagement.Domain.Rooms.CosmosDB;
using MediatR;

namespace GymManagement.Application.Rooms.Commands.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, ErrorOr<Room>>
{
    private readonly ISubscriptionsRepository _subscriptionsRepository;
    private readonly IGymsRepository _gymsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICosmosDBRoomRepository _cosmosDBRoomRepository;

    public CreateRoomCommandHandler(
        ISubscriptionsRepository subscriptionsRepository,
        IGymsRepository gymsRepository,
        IUnitOfWork unitOfWork, ICosmosDBRoomRepository cosmosDbRoomRepository)
    {
        _subscriptionsRepository = subscriptionsRepository;
        _gymsRepository = gymsRepository;
        _unitOfWork = unitOfWork;
        _cosmosDBRoomRepository = cosmosDbRoomRepository;
    }

    public async Task<ErrorOr<Room>> Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var gym = await _gymsRepository.GetByIdAsync(command.GymId);

        if (gym is null)
        {
            return Error.NotFound(description: "Gym not found");
        }

        var subscription = await _subscriptionsRepository.GetByIdAsync(gym.SubscriptionId);

        if (subscription is null)
        {
            return Error.Unexpected(description: "Subscription not found");
        }

        var room = new Room(
            name: command.RoomName,
            gymId: gym.Id,
            maxDailySessions: subscription.GetMaxDailySessions());

        var addGymResult = gym.AddRoom(room);

        if (addGymResult.IsError)
        {
            return addGymResult.Errors;
        }

        var cosmosDBRoom = new CosmosDBRoom
        {
            Id = room.Id,
            Name = room.Name,
            GymId = room.GymId,
            MaxDailySessions = room.MaxDailySessions
        };
        await _cosmosDBRoomRepository.AddItemAsync(cosmosDBRoom);

        // Note: the room itself isn't stored in our database, but rather
        // in the "SessionManagement" system that is not in scope of this course.
        await _gymsRepository.UpdateGymAsync(gym);
        await _unitOfWork.CommitChangesAsync(cancellationToken);

        return room;
    }
}