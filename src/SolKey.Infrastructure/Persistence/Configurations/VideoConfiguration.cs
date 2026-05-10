using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.Property(video => video.Title).HasMaxLength(200).IsRequired();
        builder.Property(video => video.BlobPath).HasMaxLength(500).IsRequired();
        builder.HasIndex(video => new { video.Type, video.IsApproved });
        builder.HasQueryFilter(video => !video.IsDeleted);
    }
}
