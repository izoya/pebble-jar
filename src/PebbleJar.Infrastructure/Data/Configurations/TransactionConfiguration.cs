using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PebbleJar.Domain;
using System.Globalization;

namespace PebbleJar.Infrastructure.Data.Configurations
{
    internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.Property(t => t.Description).HasMaxLength(200);

            // Current SQLite type for timestamps is TEXT.
            // This conversion ensures that dates are all written in the same UTC format,
            // making their text order match chronological order.
            builder.Property(transaction => transaction.TransactionDateTime)
                .HasConversion(
                    value => value.ToUniversalTime()
                        .ToString("O", CultureInfo.InvariantCulture),
                    value => DateTimeOffset.Parse(
                        value,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind));

            // TODO: Current Sqlite type for Amount is TEXT, which makes comparison queries funny.
            // Can store unscaled value and the scale separately, or use currency-offset dict.

            builder.HasIndex(t => new { t.AccountId, t.ExternalId }).IsUnique();

            builder.HasOne(t => t.Account)
                .WithMany(acc => acc.Transactions)
                .HasForeignKey(t => t.AccountId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(
                t => t.RecognitionData,
                rd => rd.ToJson("recognition_data"));
        }
    }
}
