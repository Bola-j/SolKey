using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(tag => tag.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(tag => tag.Name).IsUnique();
        builder.HasQueryFilter(tag => !tag.IsDeleted);
    }
}
