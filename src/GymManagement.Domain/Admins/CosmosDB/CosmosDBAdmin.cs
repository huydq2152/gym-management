using GymManagement.Domain.Common;

namespace GymManagement.Domain.Admins.CosmosDB;

public class CosmosDBAdmin : Entity
{
    public Guid UserId { get; }
    public Guid? SubscriptionId { get; set; } = null;
    public Guid Id { get; set; }
}