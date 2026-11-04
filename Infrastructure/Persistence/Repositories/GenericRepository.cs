using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
        where TEntity : BaseEntity<TKey>
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task AddAsync(TEntity entity)
            => await _dbSet.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
            => await _dbSet.AddRangeAsync(entities);

        public void Update(TEntity entity)
            => _dbSet.Update(entity);

        public void UpdateRange(IEnumerable<TEntity> entities)
            => _dbSet.UpdateRange(entities);

        public void Remove(TEntity entity)
            => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<TEntity> entities)
            => _dbSet.RemoveRange(entities);

        public async Task<TEntity?> GetByIdAsync(TKey key)
            => await _dbSet.FindAsync(key);

        public async Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            if (includes is { Length: > 0 })
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync();
        }


        public async Task<IEnumerable<TEntity>> GetAllAsync(
           Expression<Func<TEntity, bool>>? predicate = null,
           bool trackChanges = false,
           params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = trackChanges ? _dbSet : _dbSet.AsNoTracking();

            // Filter
            if (predicate != null)
                query = query.Where(predicate);

            // Includes
            if (includes is { Length: > 0 })
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            query = query.OrderBy(e => 0);

            return await query.ToListAsync();
        }



        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (predicate != null)
                query = query.Where(predicate);

            return await query.CountAsync();
        }

        public IQueryable<TEntity> GetAllQueryable(
             Expression<Func<TEntity, bool>>? predicate = null,
             params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            if (includes is { Length: > 0 })
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            return query;
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null)
            => predicate is not null? await _dbSet.AnyAsync(predicate) : await _dbSet.AnyAsync();
        public IQueryable<TEntity> GetAllAdvanced(
            Expression<Func<TEntity, bool>>? predicate = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            foreach (var include in includes)
                query = include(query);

            return query;
        }


    }
}