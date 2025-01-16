using GymManagement.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Rooms.Persistence;

public class RoomChangeConfigurations: IEntityTypeConfiguration<RoomChange>
{
    public void Configure(EntityTypeBuilder<RoomChange> builder)
    {
        builder.HasKey(rc => rc.RoomId);
        builder.Property(rc => rc.CosmosDBUpdated);
    }
}