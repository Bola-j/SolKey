using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.Property(lesson => lesson.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(lesson => new { lesson.ChapterId, lesson.Order }).IsUnique();
        builder.HasOne(lesson => lesson.Chapter)
            .WithMany(chapter => chapter.Lessons)
            .HasForeignKey(lesson => lesson.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(lesson => lesson.Questions)
            .WithOne(question => question.Lesson)
            .HasForeignKey(question => question.LessonId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(lesson => !lesson.IsDeleted);
    }
}
