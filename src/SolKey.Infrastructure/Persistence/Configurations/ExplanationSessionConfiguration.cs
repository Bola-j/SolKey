using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class ExplanationSessionConfiguration : IEntityTypeConfiguration<ExplanationSession>
{
    public void Configure(EntityTypeBuilder<ExplanationSession> builder)
    {
        builder.Property(session => session.Title).HasMaxLength(200).IsRequired();
        builder.Property(session => session.Description).HasMaxLength(2000).IsRequired();
        builder.Property(session => session.Price).HasColumnType("decimal(18,2)");
        builder.HasIndex(session => new { session.TeacherId, session.IsApproved });
        builder.HasQueryFilter(session => !session.IsDeleted);
    }
}
