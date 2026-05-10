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
        builder.HasQueryFilter(lesson => !lesson.IsDeleted);
    }
}
