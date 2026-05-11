using Microsoft.EntityFrameworkCore;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;

namespace SolKey.Infrastructure.Persistence;

public static class SeedData
{
    private static readonly DateTime SeedDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Guid StudentId = Guid.Parse("1f1a0b1b-2c8f-4f2f-9f11-1e6a2b1e0c01");
    private static readonly Guid TeacherId = Guid.Parse("2a2b1c2d-3e9f-4f3f-8a22-2f7b3c2f1d02");
    private static readonly Guid AdminId = Guid.Parse("3b3c2d3e-4fa0-4f4f-7b33-3a8c4d3f2e03");

    private static readonly Guid BookId = Guid.Parse("4c4d3e4f-5ab1-4f5f-6c44-4b9d5e4f3f04");
    private static readonly Guid ChapterId = Guid.Parse("5d5e4f50-6bc2-4f6f-5d55-5c0e6f5f4a05");
    private static readonly Guid LessonId = Guid.Parse("6e6f5061-7cd3-4f7f-4e66-6d1f706f5b06");

    private static readonly Guid QuestionId = Guid.Parse("7f706172-8de4-4f8f-3f77-7e20717f6c07");
    private static readonly Guid AnswerId = Guid.Parse("8a817283-9ef5-4f9f-2a88-8f31728f7d08");

    private static readonly Guid TagId = Guid.Parse("9b928394-0f06-40a0-1b99-9042839f8e09");
    private static readonly Guid QuestionTagId = Guid.Parse("0ca394a5-1017-40b1-0caa-a15394a09f0a");

    private static readonly Guid SessionId = Guid.Parse("1db4a5b6-1128-40c2-0dbb-b264a5b1a00b");
    private static readonly Guid SessionPurchaseId = Guid.Parse("2ec5b6c7-1239-40d3-0ecc-c375b6c2b10c");

    private static readonly Guid VideoId = Guid.Parse("3fd6c7d8-134a-40e4-0fdd-d486c7d3c20d");
    private static readonly Guid VideoTagId = Guid.Parse("40e7d8e9-145b-40f5-1fee-e597d8e4d30e");

    private static readonly Guid SubscriptionId = Guid.Parse("51f8e9fa-156c-4106-2fff-f6a8e9f5e40f");
    private static readonly Guid PaymentId = Guid.Parse("62f9fa0b-167d-4207-3000-07b9fa06f510");

    private static readonly Guid UserSessionId = Guid.Parse("73fa0b1c-178e-4308-4111-18ca0b17f611");
    private static readonly Guid RefreshTokenId = Guid.Parse("84fb1c2d-189f-4409-5222-29db1c28a712");

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = StudentId,
                FirstName = "Nora",
                LastName = "Ali",
                Email = "student@solkey.dev",
                PasswordHash = "seed-hash-student",
                PhoneNumber = "+201010001001",
                Bio = "Student focused on algebra and calculus.",
                IsEmailVerified = true,
                IsVerifiedTeacher = false,
                Role = UserRole.Student,
                CreatedAt = SeedDate,
                CreatedBy = "seed",
                IsDeleted = false
            },
            new User
            {
                Id = TeacherId,
                FirstName = "Omar",
                LastName = "Hassan",
                Email = "teacher@solkey.dev",
                PasswordHash = "seed-hash-teacher",
                PhoneNumber = "+201010001002",
                Bio = "Math teacher specializing in problem solving.",
                IsEmailVerified = true,
                IsVerifiedTeacher = true,
                Role = UserRole.Teacher,
                CreatedAt = SeedDate,
                CreatedBy = "seed",
                IsDeleted = false
            },
            new User
            {
                Id = AdminId,
                FirstName = "Laila",
                LastName = "Salem",
                Email = "admin@solkey.dev",
                PasswordHash = "seed-hash-admin",
                PhoneNumber = "+201010001003",
                Bio = "Platform administrator.",
                IsEmailVerified = true,
                IsVerifiedTeacher = false,
                Role = UserRole.Admin,
                CreatedAt = SeedDate,
                CreatedBy = "seed",
                IsDeleted = false
            });

        modelBuilder.Entity<Book>().HasData(new Book
        {
            Id = BookId,
            Title = "Algebra Foundations",
            Description = "A guided introduction to core algebra concepts and techniques.",
            CoverImage = "seed/books/algebra-foundations.png",
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<Chapter>().HasData(new Chapter
        {
            Id = ChapterId,
            BookId = BookId,
            Title = "Linear Equations",
            Order = 1,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<Lesson>().HasData(new Lesson
        {
            Id = LessonId,
            ChapterId = ChapterId,
            Title = "Solving One-Variable Equations",
            Order = 1,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<Question>().HasData(new Question
        {
            Id = QuestionId,
            LessonId = LessonId,
            StudentId = StudentId,
            Text = "How do I isolate x in 3x + 5 = 20?",
            Tags = "algebra,equations",
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<Answer>().HasData(new Answer
        {
            Id = AnswerId,
            QuestionId = QuestionId,
            TeacherId = TeacherId,
            Text = "Subtract 5, then divide by 3 to get x = 5.",
            Upvotes = 12,
            Downvotes = 1,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<Tag>().HasData(new Tag
        {
            Id = TagId,
            Name = "algebra",
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<QuestionTag>().HasData(new QuestionTag
        {
            Id = QuestionTagId,
            QuestionId = QuestionId,
            TagId = TagId,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<ExplanationSession>().HasData(new ExplanationSession
        {
            Id = SessionId,
            Title = "Linear Equations Deep Dive",
            Description = "Live walkthroughs with practice problems and feedback.",
            Price = 19.99m,
            TeacherId = TeacherId,
            AccessDurationDays = 14,
            IsApproved = true,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<SessionPurchase>().HasData(new SessionPurchase
        {
            Id = SessionPurchaseId,
            UserId = StudentId,
            SessionId = SessionId,
            PurchasedAt = SeedDate.AddDays(1),
            ExpiresAt = SeedDate.AddDays(15),
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<Video>().HasData(new Video
        {
            Id = VideoId,
            Title = "Solving 3x + 5 = 20",
            Type = VideoType.QuestionSolution,
            BlobPath = "seed/videos/solve-3x-plus-5.mp4",
            IsPremium = false,
            IsApproved = true,
            TeacherId = TeacherId,
            QuestionId = QuestionId,
            SessionId = null,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<VideoTag>().HasData(new VideoTag
        {
            Id = VideoTagId,
            VideoId = VideoId,
            TagId = TagId,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<Subscription>().HasData(new Subscription
        {
            Id = SubscriptionId,
            UserId = StudentId,
            StartDate = SeedDate,
            EndDate = SeedDate.AddMonths(1),
            IsActive = true,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<Payment>().HasData(new Payment
        {
            Id = PaymentId,
            UserId = StudentId,
            Amount = 9.99m,
            Type = PaymentType.QaSubscription,
            ScreenshotPath = "seed/payments/subscription.png",
            IsApproved = true,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<UserSession>().HasData(new UserSession
        {
            Id = UserSessionId,
            UserId = StudentId,
            DeviceId = "seed-device-001",
            DeviceName = "Seed Laptop",
            IPAddress = "127.0.0.1",
            IsActive = true,
            LastSeenAt = SeedDate.AddDays(2),
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });

        modelBuilder.Entity<RefreshToken>().HasData(new RefreshToken
        {
            Id = RefreshTokenId,
            UserId = StudentId,
            SessionId = UserSessionId,
            Token = "seed-refresh-token-001",
            ExpiresAt = SeedDate.AddDays(30),
            IsRevoked = false,
            CreatedAt = SeedDate,
            CreatedBy = "seed",
            IsDeleted = false
        });
    }
}
