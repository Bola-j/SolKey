using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverImage { get; set; } = string.Empty;

    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}
