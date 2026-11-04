using Domain.Models;
using System.Linq.Expressions;

namespace Domain.Contracts
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        /// <summary>
        /// Adds a single entity to the database asynchronously.
        /// </summary>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Adds a collection of entities to the database asynchronously.
        /// </summary>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Updates a single entity in the database.
        /// </summary>
        void Update(TEntity entity);

        /// <summary>
        /// Updates a collection of entities in the database.
        /// </summary>
        void UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Removes a single entity from the database.
        /// </summary>
        void Remove(TEntity entity);

        /// <summary>
        /// Removes a collection of entities from the database.
        /// </summary>
        void RemoveRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Finds an entity by its primary key asynchronously.
        /// </summary>
        Task<TEntity?> GetByIdAsync(TKey key);

        /// <summary>
        /// Retrieves the first entity that matches the given predicate, 
        /// optionally including navigation properties.
        /// </summary>
        Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Retrieves all entities that match the given predicate asynchronously, 
        /// optionally including navigation properties and enabling/disabling change tracking.
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool trackChanges = false,
            params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Counts the number of entities that match the given predicate asynchronously.
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

        /// <summary>
        /// Checks if any entity matches the given predicate asynchronously.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null);

        /// <summary>
        /// Returns an <see cref="IQueryable{TEntity}"/> to allow further filtering, 
        /// sorting, and pagination before execution.
        /// </summary>
        IQueryable<TEntity> GetAllQueryable(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includes);
        IQueryable<TEntity> GetAllAdvanced(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes);

    }
}
