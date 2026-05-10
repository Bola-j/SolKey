using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class Answer : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Guid TeacherId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }

    public Question Question { get; set; } = null!;
    public User Teacher { get; set; } = null!;
}
