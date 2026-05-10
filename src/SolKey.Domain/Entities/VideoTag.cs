using SolKey.Domain.Common;

namespace SolKey.Domain.Entities;

public class VideoTag : BaseEntity
{
    public Guid VideoId { get; set; }
    public Guid TagId { get; set; }

    public Video Video { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
