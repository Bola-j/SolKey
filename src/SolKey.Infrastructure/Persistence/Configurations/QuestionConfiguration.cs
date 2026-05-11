using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.Property(question => question.Text).HasMaxLength(4000).IsRequired();
        builder.Property(question => question.Tags).HasMaxLength(500);
        builder.HasIndex(question => question.LessonId);
        builder.HasOne(question => question.Lesson)
            .WithMany(lesson => lesson.Questions)
            .HasForeignKey(question => question.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(question => question.Student)
            .WithMany(user => user.Questions)
            .HasForeignKey(question => question.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(question => question.Answers)
            .WithOne(answer => answer.Question)
            .HasForeignKey(answer => answer.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(question => question.Videos)
            .WithOne(video => video.Question)
            .HasForeignKey(video => video.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(question => !question.IsDeleted);
    }
}
