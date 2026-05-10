using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class Question : BaseEntity
{
    public Guid LessonId { get; set; }
    public Guid StudentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Tags { get; set; }

    public Lesson Lesson { get; set; } = null!;
    public User Student { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    public ICollection<Video> Videos { get; set; } = new List<Video>();
}
