using SolKey.Domain.Common;
using SolKey.Domain.Enums;

namespace SolKey.Domain.Entities;

public class Video : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public VideoType Type { get; set; }
    public string BlobPath { get; set; } = string.Empty;
    public bool IsPremium { get; set; }
    public bool IsApproved { get; set; }
    public Guid TeacherId { get; set; }
    public Guid? QuestionId { get; set; }
    public Guid? SessionId { get; set; }

    public User Teacher { get; set; } = null!;
    public Question? Question { get; set; }
    public ExplanationSession? Session { get; set; }
    public ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();
}
