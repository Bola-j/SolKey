using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class QuestionTag : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Guid TagId { get; set; }

    public Question Question { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
