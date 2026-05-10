using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class SessionPurchaseConfiguration : IEntityTypeConfiguration<SessionPurchase>
{
    public void Configure(EntityTypeBuilder<SessionPurchase> builder)
    {
        builder.HasIndex(purchase => new { purchase.UserId, purchase.SessionId });
        builder.HasQueryFilter(purchase => !purchase.IsDeleted);
    }
}
