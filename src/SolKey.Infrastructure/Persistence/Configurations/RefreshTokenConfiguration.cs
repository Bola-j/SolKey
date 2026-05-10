using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(token => token.Token).HasMaxLength(500).IsRequired();
        builder.HasIndex(token => token.Token).IsUnique();
        builder.HasQueryFilter(token => !token.IsDeleted);
    }
}
