using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(token => token.Purpose).HasMaxLength(100).IsRequired();

        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.ExpiresAt);
        builder.HasIndex(token => new { token.UserId, token.Purpose });

        builder.HasOne(token => token.User)
            .WithMany(user => user.EmailVerificationTokens)
            .HasForeignKey(token => token.UserId);

        builder.HasQueryFilter(token => !token.IsDeleted);
    }
}
