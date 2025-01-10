using Ardalis.Specification;
using GymManagement.Domain.Common.CosmosDB;

namespace GymManagement.Application.Common.Interfaces.CosmosDB
{
    public interface ICosmosDBRepository<T> where T : CosmosDBEntity
    {
        /// <summary>
        ///     Get items given a string SQL query directly.
        ///     Likely in production, you may want to use alternatives like Parameterized Query or LINQ to avoid SQL Injection and avoid having to work with strings directly.
        ///     This is kept here for demonstration purpose.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetItemsAsync(string query);
        /// <summary>
        ///     Get items given a specification.
        /// </summary>
        /// <param name="specification"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetItemsAsync(ISpecification<T> specification);

        /// <summary>
        ///     Get the count on items that match the specification
        /// </summary>
        /// <param name="specification"></param>
        /// <returns></returns>
        Task<int> GetItemsCountAsync(ISpecification<T> specification);
        
        /// <summary>
        /// Retrieves an item from the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="resolvePartitionKeyInput">The input used to resolve the partition key.</param>
        /// <returns>A task that represents the asynchronous operation, containing the retrieved item.</returns>
        Task<T> GetItemAsync(Guid id, Guid resolvePartitionKeyInput);
        
        /// <summary>
        /// Adds an item to the Cosmos DB container.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="resolvePartitionKeyInput">The input used to resolve the partition key.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddItemAsync(T item, Guid resolvePartitionKeyInput);

        /// <summary>
        /// Updates an item in the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the item to update.</param>
        /// <param name="item">The updated item.</param>
        /// <param name="resolvePartitionKeyInput">The input used to resolve the partition key.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateItemAsync(Guid id, T item, Guid resolvePartitionKeyInput);

        /// <summary>
        /// Deletes an item from the Cosmos DB container.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <param name="resolvePartitionKeyInput">The input used to resolve the partition key.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteItemAsync(Guid id, Guid resolvePartitionKeyInput);
    }
}
