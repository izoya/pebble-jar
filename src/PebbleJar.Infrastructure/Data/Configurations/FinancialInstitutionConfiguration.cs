using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PebbleJar.Domain;

namespace PebbleJar.Infrastructure.Data.Configurations;

internal class FinancialInstitutionConfiguration
    : IEntityTypeConfiguration<FinancialInstitution>
{
    public void Configure(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
