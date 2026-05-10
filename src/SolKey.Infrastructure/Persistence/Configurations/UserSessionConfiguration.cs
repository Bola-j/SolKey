using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.Property(session => session.DeviceId).HasMaxLength(200).IsRequired();
        builder.Property(session => session.DeviceName).HasMaxLength(200).IsRequired();
        builder.Property(session => session.IPAddress).HasMaxLength(45).IsRequired();
        builder.HasIndex(session => new { session.UserId, session.IsActive });
        builder.HasQueryFilter(session => !session.IsDeleted);
    }
}
