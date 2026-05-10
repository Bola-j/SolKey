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
        builder.HasQueryFilter(question => !question.IsDeleted);
    }
}
