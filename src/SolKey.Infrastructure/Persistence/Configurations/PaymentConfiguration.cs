using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(payment => payment.ScreenshotPath).HasMaxLength(500).IsRequired();
        builder.Property(payment => payment.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(payment => new { payment.UserId, payment.Type });
        builder.HasQueryFilter(payment => !payment.IsDeleted);
    }
}
