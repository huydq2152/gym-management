using GymManagement.Domain.Rooms;

namespace GymManagement.Application.Common.Interfaces;

public interface IRoomChangeRepository
{
    Task AddRoomChangeAsync(RoomChange roomChange);
    Task<RoomChange?> GetByRoomIdAsync(Guid roomId);
    Task UpdateRoomChangeAsync(RoomChange roomChange);
}