using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.Property(chapter => chapter.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(chapter => new { chapter.BookId, chapter.Order }).IsUnique();
        builder.HasOne(chapter => chapter.Book)
            .WithMany(book => book.Chapters)
            .HasForeignKey(chapter => chapter.BookId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(chapter => chapter.Lessons)
            .WithOne(lesson => lesson.Chapter)
            .HasForeignKey(lesson => lesson.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(chapter => !chapter.IsDeleted);
    }
}
