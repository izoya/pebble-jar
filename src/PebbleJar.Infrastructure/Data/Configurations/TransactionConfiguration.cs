using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PebbleJar.Domain;

namespace PebbleJar.Infrastructure.Data.Configurations
{
    internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.Property(t => t.Description).HasMaxLength(200);

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
