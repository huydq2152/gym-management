namespace GymManagement.Domain.Common.CosmosDB;

public class CosmosDBAudit : Entity
{
    /// <summary>
    ///     Type of the entity
    /// </summary>
    public string EntityType { get; set; }

    /// <summary>
    ///     Entity Id.
    ///     Use this as the Partition Key, so that all the auditing records for the same entity are stored in the same logical partition.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    ///     Entity itself
    /// </summary>
    public string Entity { get; set; }

    /// <summary>
    ///     Date audit record created
    /// </summary>
    public DateTime DateCreatedUTC { get; set; }
}