using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class VideoTagConfiguration : IEntityTypeConfiguration<VideoTag>
{
    public void Configure(EntityTypeBuilder<VideoTag> builder)
    {
        builder.HasIndex(mapping => new { mapping.VideoId, mapping.TagId }).IsUnique();
        builder.HasQueryFilter(mapping => !mapping.IsDeleted);
    }
}
