using System.Net;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using GymManagement.Application.Common.Interfaces.CosmosDB;
using GymManagement.Domain.Admins.CosmosDB;
using GymManagement.Domain.Common;
using GymManagement.Domain.Common.CosmosDB;
using GymManagement.Infrastructure.Common.Persistence.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;

namespace GymManagement.Infrastructure.Common.Persistence.CosmosDb
{
    public abstract class CosmosDbRepository<T> : ICosmosDBRepository<T>, IContainerContext<T> where T : Entity
    {
        /// <summary>
        ///     Name of the CosmosDB container
        /// </summary>
        public abstract string ContainerName { get; }

        /// <summary>
        ///     Generate id
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract Guid GenerateId(T entity);

        /// <summary>
        ///     Resolve the partition key
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public abstract PartitionKey ResolvePartitionKey(Guid entityId);

        /// <summary>
        ///     Generate id for the audit record.
        ///     All entities will share the same audit container,
        ///     so we can define this method here with virtual default implementation.
        ///     Audit records for different entities will use different partition key values,
        ///     so we are not limited to the 20G per logical partition storage limit.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual Guid GenerateAuditId(CosmosDBAudit entity) => Guid.NewGuid();

        /// <summary>
        ///     Resolve the partition key for the audit record.
        ///     All entities will share the same audit container,
        ///     so we can define this method here with virtual default implementation.
        ///     Audit records for different entities will use different partition key values,
        ///     so we are not limited to the 20G per logical partition storage limit.
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public virtual PartitionKey ResolveAuditPartitionKey(Guid entityId) => new PartitionKey(entityId.ToString());

        /// <summary>
        ///     Cosmos DB factory
        /// </summary>
        private readonly ICosmosDbContainerFactory _cosmosDbContainerFactory;

        /// <summary>
        ///     Cosmos DB container
        /// </summary>
        private readonly Container _container;

        /// <summary>
        ///     Audit container that will store audit log for all entities.
        /// </summary>
        private readonly Container _auditContainer;

        public CosmosDbRepository(ICosmosDbContainerFactory cosmosDbContainerFactory)
        {
            _cosmosDbContainerFactory = cosmosDbContainerFactory ??
                                        throw new ArgumentNullException(nameof(ICosmosDbContainerFactory));
            _container = _cosmosDbContainerFactory.GetContainer(ContainerName).Container;
            _auditContainer = _cosmosDbContainerFactory.GetContainer("Audit").Container;
        }

        public async Task AddItemAsync(T item)
        {
            item.Id = GenerateId(item);
            await _container.CreateItemAsync(item, ResolvePartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(Guid id)
        {
            await _container.DeleteItemAsync<T>(id.ToString(), ResolvePartitionKey(id));
        }

        public async Task<T> GetItemAsync(Guid id)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(id.ToString(), ResolvePartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        // Search data using SQL query string
        // This shows how to use SQL string to read data from Cosmos DB for demonstration purpose.
        // For production, try to use safer alternatives like Parameterized Query and LINQ if possible.
        // Using string can expose SQL Injection vulnerability, e.g. select * from c where c.id=1 OR 1=1. 
        // String can also be hard to work with due to special characters and spaces when advanced querying like search and pagination is required.
        public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
        {
            var resultSetIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            var results = new List<T>();
            while (resultSetIterator.HasMoreResults)
            {
                var response = await resultSetIterator.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        /// <inheritdoc cref="ICosmosDBRepository{T}.GetItemsAsync(Ardalis.Specification.ISpecification{T})"/>
        public async Task<IEnumerable<T>> GetItemsAsync(ISpecification<T> specification)
        {
            var queryable = ApplySpecification(specification);
            var iterator = queryable.ToFeedIterator();

            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        /// <inheritdoc cref="ICosmosDBRepository{T}.GetItemsCountAsync(ISpecification{T})"/>
        public async Task<int> GetItemsCountAsync(ISpecification<T> specification)
        {
            var queryable = ApplySpecification(specification);
            return await queryable.CountAsync();
        }

        public async Task UpdateItemAsync(Guid id, T item)
        {
            // Audit
            await Audit(item);
            // Update
            await _container.UpsertItemAsync(item, ResolvePartitionKey(id));
        }

        /// <summary>
        ///     Evaluate specification and return IQueryable
        /// </summary>
        /// <param name="specification"></param>
        /// <returns></returns>
        private IQueryable<T> ApplySpecification(ISpecification<T> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_container.GetItemLinqQueryable<T>(), specification);
        }

        /// <summary>
        ///     Audit a item by adding it to the audit container
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task Audit(T item)
        {
            var auditItem = new CosmosDBAudit
            {
                EntityType = item.GetType().Name,
                EntityId = item.Id,
                Entity = JsonConvert.SerializeObject(item)
            };
            auditItem.Id = GenerateAuditId(auditItem);
            await _auditContainer.CreateItemAsync(auditItem, ResolveAuditPartitionKey(auditItem.Id));
        }
    }
}