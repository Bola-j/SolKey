using Microsoft.EntityFrameworkCore;
using SolKey.Domain.Common;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence;

public class SolKeyDbContext : DbContext
{
    public SolKeyDbContext(DbContextOptions<SolKeyDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<ExplanationSession> ExplanationSessions => Set<ExplanationSession>();
    public DbSet<SessionPurchase> SessionPurchases => Set<SessionPurchase>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<QuestionTag> QuestionTags => Set<QuestionTag>();
    public DbSet<VideoTag> VideoTags => Set<VideoTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SolKeyDbContext).Assembly);
        SeedData.Apply(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    private void ApplyAuditInfo()
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.ModifiedAt = null;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = utcNow;
                    break;
                case EntityState.Deleted:
                    entry.Entity.IsDeleted = true;
                    break;
            }
        }
    }
}
