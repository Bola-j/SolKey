using SolKey.Domain.Common;
using SolKey.Domain.Enums;

namespace SolKey.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsVerifiedTeacher { get; set; }
    public UserRole Role { get; set; }

    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<SessionPurchase> SessionPurchases { get; set; } = new List<SessionPurchase>();
    public ICollection<Video> Videos { get; set; } = new List<Video>();
    public ICollection<ExplanationSession> ExplanationSessions { get; set; } = new List<ExplanationSession>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
