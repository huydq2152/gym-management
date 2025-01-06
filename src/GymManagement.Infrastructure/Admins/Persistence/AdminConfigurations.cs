using GymManagement.Domain.Admins;
using GymManagement.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Infrastructure.Admins.Persistence;

public class AdminConfigurations : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .ValueGeneratedNever();
        
        builder.Property(a => a.UserId);
        builder.Property(a => a.SubscriptionId);
        
        builder.HasData(new Admin(
            userId: UserConstants.Id,
            subscriptionId: null,
            id: AdminConstants.Id));
    }
}
