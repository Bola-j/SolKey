using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class Lesson : BaseEntity
{
    public Guid ChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }

    public Chapter Chapter { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
