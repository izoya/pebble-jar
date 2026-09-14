using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PebbleJar.Infrastructure.Data.Configurations;

internal sealed class TransactionLocalDateConfiguration
    : IEntityTypeConfiguration<TransactionLocalDate>
{
    public void Configure(EntityTypeBuilder<TransactionLocalDate> builder)
    {
        builder.HasNoKey();
        builder.ToView("transaction_local_dates");
    }
}
