using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class UploadTicketConfiguration : IEntityTypeConfiguration<UploadTicket>
{
    public void Configure(EntityTypeBuilder<UploadTicket> builder)
    {
        builder.ToTable("UploadTickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BlobPath)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.Purpose)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ContentType)
            .HasMaxLength(100);

        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => new { x.UserId, x.Purpose });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
