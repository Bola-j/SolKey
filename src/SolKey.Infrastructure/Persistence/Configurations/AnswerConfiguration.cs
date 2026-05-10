using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.Property(answer => answer.Text).HasMaxLength(4000).IsRequired();
        builder.HasIndex(answer => new { answer.QuestionId, answer.TeacherId });
        builder.HasQueryFilter(answer => !answer.IsDeleted);
    }
}
