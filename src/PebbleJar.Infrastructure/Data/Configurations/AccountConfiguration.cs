using Microsoft.EntityFrameworkCore;
using PebbleJar.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace PebbleJar.Infrastructure.Data.Configurations
{
    internal class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Account> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(200);
            builder.Property(x => x.AccountNumber)
                .HasMaxLength(100);

            // SQLite permits multiple NULL values in a unique index.
            builder.HasIndex(x => new { x.ConnectionProvider, x.ExternalId })
                .IsUnique();

            builder.HasOne(x => x.FinancialInstitution)
                .WithMany(x => x.Accounts)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
