using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class Chapter : BaseEntity
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }

    public Book Book { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
