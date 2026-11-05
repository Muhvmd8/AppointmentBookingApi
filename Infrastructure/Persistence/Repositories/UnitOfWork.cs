using Domain.Contracts;
using Domain.Models;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class UnitOfWork(ApplicationDbContext _context)
    : IUnitOfWork
    {
        private readonly Dictionary<string/*typeName*/, object/*Repo*/> _repositories = [];

        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : BaseEntity<TKey>
        {
            var typeName = typeof(TEntity).Name;
            // Get repo from the container
            if (_repositories.ContainsKey(typeName))
                return (IGenericRepository<TEntity, TKey>)_repositories[typeName];

            var repo = new GenericRepository<TEntity, TKey>(_context);
            _repositories[typeName] = repo; // Add to the container
            return repo;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_context.Database.CurrentTransaction is null)
                await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_context.Database.CurrentTransaction is not null)
                await _context.Database.CurrentTransaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_context.Database.CurrentTransaction is not null)
                await _context.Database.CurrentTransaction.RollbackAsync(cancellationToken);
        }
    }
}
