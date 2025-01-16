using GymManagement.Application.Common.Interfaces;
using GymManagement.Domain.Rooms;
using GymManagement.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Rooms.Persistence;

public class RoomChangeRepository : IRoomChangeRepository
{
    private readonly GymManagementDbContext _dbContext;

    public RoomChangeRepository(GymManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRoomChangeAsync(RoomChange roomChange)
    {
        await _dbContext.RoomChanges.AddAsync(roomChange);
    }

    public async Task<RoomChange?> GetByRoomIdAsync(Guid roomId)
    {
        return await _dbContext.RoomChanges.FirstOrDefaultAsync(roomChange => roomChange.RoomId == roomId);
    }

    public Task UpdateRoomChangeAsync(RoomChange roomChange)
    {
        _dbContext.Update(roomChange);
        return Task.CompletedTask;
    }
}