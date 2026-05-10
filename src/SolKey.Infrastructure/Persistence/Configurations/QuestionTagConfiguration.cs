using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class QuestionTagConfiguration : IEntityTypeConfiguration<QuestionTag>
{
    public void Configure(EntityTypeBuilder<QuestionTag> builder)
    {
        builder.HasIndex(mapping => new { mapping.QuestionId, mapping.TagId }).IsUnique();
        builder.HasQueryFilter(mapping => !mapping.IsDeleted);
    }
}
