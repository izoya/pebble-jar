using Microsoft.EntityFrameworkCore;
using PebbleJar.Domain;
using PebbleJar.Domain.Abstractions;

namespace PebbleJar.Infrastructure.Data;

public sealed class PebbleJarDbContext(
    DbContextOptions<PebbleJarDbContext> options)
    : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    //public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<FinancialInstitution> FinancialInstitutions
        => Set<FinancialInstitution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PebbleJarDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
             // equivalent of: if (entity is IAuditable)
             .Where(type => typeof(IAuditable).IsAssignableFrom(type.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTimeOffset>("CreatedAt")
                .IsRequired();

            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTimeOffset>("UpdatedAt")
                .IsRequired();
        }
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps();


        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void ApplyAuditTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = ChangeTracker.Entries()
            .Where(entry =>
                entry.Entity is IAuditable &&
                entry.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = now;
            }

            entry.Property("UpdatedAt").CurrentValue = now;
        }
    }
}
