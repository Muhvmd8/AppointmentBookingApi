using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Persistence.Data;

namespace Persistence.Seeding
{
    public class DataSeeding(
        ApplicationDbContext _context,
        IHostEnvironment _env
        ) : IDataSeeding
    {
        public void DataSeed()
        {
            if (!_env.IsDevelopment())
                if (_context.Database.GetPendingMigrations().Any())
                    _context.Database.Migrate();

            //_typeSeeder.SeedAsync().GetAwaiter().GetResult();

            _context.SaveChanges();
        }
    }
}
