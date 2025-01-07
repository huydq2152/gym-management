using Ardalis.Specification;

namespace GymManagement.Domain.Common.CosmosDB.Specifications
{
    public sealed class AuditFilterSpecification : Specification<CosmosDBAudit>
    {
        /// <summary>
        ///     Search by a matching entity Id
        /// </summary>
        /// <param name="entityId"></param>
        public AuditFilterSpecification(Guid entityId)
        {
            Query.Where(audit =>
                // Must include EntityId, because it is part of the Partition Key
                audit.EntityId == entityId)
                .OrderByDescending(audit => audit.DateCreatedUTC);
        }
    }
}
