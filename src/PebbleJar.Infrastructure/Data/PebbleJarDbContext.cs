using Microsoft.EntityFrameworkCore;
using PebbleJar.Domain;

namespace PebbleJar.Infrastructure.Data
{
    public sealed class PebbleJarDbContext(DbContextOptions<PebbleJarDbContext> options) 
        : DbContext(options)
    {
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
    }
}
