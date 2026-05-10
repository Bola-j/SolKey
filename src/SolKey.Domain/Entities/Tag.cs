using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    public ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();
}
