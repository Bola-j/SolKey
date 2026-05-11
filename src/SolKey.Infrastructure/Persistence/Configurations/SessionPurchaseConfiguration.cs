using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class SessionPurchaseConfiguration : IEntityTypeConfiguration<SessionPurchase>
{
    public void Configure(EntityTypeBuilder<SessionPurchase> builder)
    {
        builder.HasIndex(purchase => new { purchase.UserId, purchase.SessionId });
        builder.HasOne(purchase => purchase.User)
            .WithMany(user => user.SessionPurchases)
            .HasForeignKey(purchase => purchase.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(purchase => purchase.Session)
            .WithMany(session => session.Purchases)
            .HasForeignKey(purchase => purchase.SessionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(purchase => !purchase.IsDeleted);
    }
}
