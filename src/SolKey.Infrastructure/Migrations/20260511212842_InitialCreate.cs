using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SolKey.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CoverImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsVerifiedTeacher = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chapters_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExplanationSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessDurationDays = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExplanationSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExplanationSessions_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ScreenshotPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lessons_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionPurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionPurchases_ExplanationSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ExplanationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionPurchases_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_UserSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Questions_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Upvotes = table.Column<int>(type: "int", nullable: false),
                    Downvotes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Answers_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionTags_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPremium = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_ExplanationSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ExplanationSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Videos_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Videos_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoTags_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "CoverImage", "CreatedAt", "CreatedBy", "Description", "IsDeleted", "ModifiedAt", "ModifiedBy", "Title" },
                values: new object[] { new Guid("4c4d3e4f-5ab1-4f5f-6c44-4b9d5e4f3f04"), "seed/books/algebra-foundations.png", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "A guided introduction to core algebra concepts and techniques.", false, null, null, "Algebra Foundations" });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[] { new Guid("9b928394-0f06-40a0-1b99-9042839f8e09"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", false, null, null, "algebra" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Bio", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsDeleted", "IsEmailVerified", "IsVerifiedTeacher", "LastName", "ModifiedAt", "ModifiedBy", "PasswordHash", "PhoneNumber", "PhotoUrl", "Role" },
                values: new object[,]
                {
                    { new Guid("1f1a0b1b-2c8f-4f2f-9f11-1e6a2b1e0c01"), "Student focused on algebra and calculus.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "student@solkey.dev", "Nora", false, true, false, "Ali", null, null, "seed-hash-student", "+201010001001", null, 1 },
                    { new Guid("2a2b1c2d-3e9f-4f3f-8a22-2f7b3c2f1d02"), "Math teacher specializing in problem solving.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "teacher@solkey.dev", "Omar", false, true, true, "Hassan", null, null, "seed-hash-teacher", "+201010001002", null, 2 },
                    { new Guid("3b3c2d3e-4fa0-4f4f-7b33-3a8c4d3f2e03"), "Platform administrator.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "admin@solkey.dev", "Laila", false, true, false, "Salem", null, null, "seed-hash-admin", "+201010001003", null, 3 }
                });

            migrationBuilder.InsertData(
                table: "Chapters",
                columns: new[] { "Id", "BookId", "CreatedAt", "CreatedBy", "IsDeleted", "ModifiedAt", "ModifiedBy", "Order", "Title" },
                values: new object[] { new Guid("5d5e4f50-6bc2-4f6f-5d55-5c0e6f5f4a05"), new Guid("4c4d3e4f-5ab1-4f5f-6c44-4b9d5e4f3f04"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", false, null, null, 1, "Linear Equations" });

            migrationBuilder.InsertData(
                table: "ExplanationSessions",
                columns: new[] { "Id", "AccessDurationDays", "CreatedAt", "CreatedBy", "Description", "IsApproved", "IsDeleted", "ModifiedAt", "ModifiedBy", "Price", "TeacherId", "Title" },
                values: new object[] { new Guid("1db4a5b6-1128-40c2-0dbb-b264a5b1a00b"), 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "Live walkthroughs with practice problems and feedback.", true, false, null, null, 19.99m, new Guid("2a2b1c2d-3e9f-4f3f-8a22-2f7b3c2f1d02"), "Linear Equations Deep Dive" });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "Amount", "CreatedAt", "CreatedBy", "IsApproved", "IsDeleted", "ModifiedAt", "ModifiedBy", "ScreenshotPath", "Type", "UserId" },
                values: new object[] { new Guid("62f9fa0b-167d-4207-3000-07b9fa06f510"), 9.99m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", true, false, null, null, "seed/payments/subscription.png", 1, new Guid("1f1a0b1b-2c8f-4f2f-9f11-1e6a2b1e0c01") });

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "EndDate", "IsActive", "IsDeleted", "ModifiedAt", "ModifiedBy", "StartDate", "UserId" },
                values: new object[] { new Guid("51f8e9fa-156c-4106-2fff-f6a8e9f5e40f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1f1a0b1b-2c8f-4f2f-9f11-1e6a2b1e0c01") });

            migrationBuilder.InsertData(
                table: "UserSessions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeviceId", "DeviceName", "IPAddress", "IsActive", "IsDeleted", "LastSeenAt", "ModifiedAt", "ModifiedBy", "UserId" },
                values: new object[] { new Guid("73fa0b1c-178e-4308-4111-18ca0b17f611"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "seed-device-001", "Seed Laptop", "127.0.0.1", true, false, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new Guid("1f1a0b1b-2c8f-4f2f-9f11-1e6a2b1e0c01") });

            migrationBuilder.InsertData(
                table: "Lessons",
                columns: new[] { "Id", "ChapterId", "CreatedAt", "CreatedBy", "IsDeleted", "ModifiedAt", "ModifiedBy", "Order", "Title" },
                values: new object[] { new Guid("6e6f5061-7cd3-4f7f-4e66-6d1f706f5b06"), new Guid("5d5e4f50-6bc2-4f6f-5d55-5c0e6f5f4a05"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", false, null, null, 1, "Solving One-Variable Equations" });

            migrationBuilder.InsertData(
                table: "RefreshTokens",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "ExpiresAt", "IsDeleted", "IsRevoked", "ModifiedAt", "ModifiedBy", "SessionId", "Token", "UserId" },
                values: new object[] { new Guid("84fb1c2d-189f-4409-5222-29db1c28a712"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), false, false, null, null, new Guid("73fa0b1c-178e-4308-4111-18ca0b17f611"), "seed-refresh-token-001", new Guid("1f1a0b1b-2c8f-4f2f-9f11-1e6a2b1e0c01") });

            migrationBuilder.InsertData(
                table: "SessionPurchases",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "ExpiresAt", "IsDeleted", "ModifiedAt", "ModifiedBy", "PurchasedAt", "SessionId", "UserId" },
                values: new object[] { new Guid("2ec5b6c7-1239-40d3-0ecc-c375b6c2b10c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), false, null, null, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1db4a5b6-1128-40c2-0dbb-b264a5b1a00b"), new Guid("1f1a0b1b-2c8f-4f2f-9f11-1e6a2b1e0c01") });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "LessonId", "ModifiedAt", "ModifiedBy", "StudentId", "Tags", "Text" },
                values: new object[] { new Guid("7f706172-8de4-4f8f-3f77-7e20717f6c07"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", false, new Guid("6e6f5061-7cd3-4f7f-4e66-6d1f706f5b06"), null, null, new Guid("1f1a0b1b-2c8f-4f2f-9f11-1e6a2b1e0c01"), "algebra,equations", "How do I isolate x in 3x + 5 = 20?" });

            migrationBuilder.InsertData(
                table: "Answers",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Downvotes", "IsDeleted", "ModifiedAt", "ModifiedBy", "QuestionId", "TeacherId", "Text", "Upvotes" },
                values: new object[] { new Guid("8a817283-9ef5-4f9f-2a88-8f31728f7d08"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", 1, false, null, null, new Guid("7f706172-8de4-4f8f-3f77-7e20717f6c07"), new Guid("2a2b1c2d-3e9f-4f3f-8a22-2f7b3c2f1d02"), "Subtract 5, then divide by 3 to get x = 5.", 12 });

            migrationBuilder.InsertData(
                table: "QuestionTags",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "ModifiedAt", "ModifiedBy", "QuestionId", "TagId" },
                values: new object[] { new Guid("0ca394a5-1017-40b1-0caa-a15394a09f0a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", false, null, null, new Guid("7f706172-8de4-4f8f-3f77-7e20717f6c07"), new Guid("9b928394-0f06-40a0-1b99-9042839f8e09") });

            migrationBuilder.InsertData(
                table: "Videos",
                columns: new[] { "Id", "BlobPath", "CreatedAt", "CreatedBy", "IsApproved", "IsDeleted", "IsPremium", "ModifiedAt", "ModifiedBy", "QuestionId", "SessionId", "TeacherId", "Title", "Type" },
                values: new object[] { new Guid("3fd6c7d8-134a-40e4-0fdd-d486c7d3c20d"), "seed/videos/solve-3x-plus-5.mp4", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", true, false, false, null, null, new Guid("7f706172-8de4-4f8f-3f77-7e20717f6c07"), null, new Guid("2a2b1c2d-3e9f-4f3f-8a22-2f7b3c2f1d02"), "Solving 3x + 5 = 20", 1 });

            migrationBuilder.InsertData(
                table: "VideoTags",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "ModifiedAt", "ModifiedBy", "TagId", "VideoId" },
                values: new object[] { new Guid("40e7d8e9-145b-40f5-1fee-e597d8e4d30e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", false, null, null, new Guid("9b928394-0f06-40a0-1b99-9042839f8e09"), new Guid("3fd6c7d8-134a-40e4-0fdd-d486c7d3c20d") });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId_TeacherId",
                table: "Answers",
                columns: new[] { "QuestionId", "TeacherId" });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_TeacherId",
                table: "Answers",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_BookId_Order",
                table: "Chapters",
                columns: new[] { "BookId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExplanationSessions_TeacherId_IsApproved",
                table: "ExplanationSessions",
                columns: new[] { "TeacherId", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ChapterId_Order",
                table: "Lessons",
                columns: new[] { "ChapterId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId_Type",
                table: "Payments",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_LessonId",
                table: "Questions",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_StudentId",
                table: "Questions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTags_QuestionId_TagId",
                table: "QuestionTags",
                columns: new[] { "QuestionId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTags_TagId",
                table: "QuestionTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_SessionId",
                table: "RefreshTokens",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPurchases_SessionId",
                table: "SessionPurchases",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPurchases_UserId_SessionId",
                table: "SessionPurchases",
                columns: new[] { "UserId", "SessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_IsActive",
                table: "Subscriptions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_IsActive",
                table: "UserSessions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Videos_QuestionId",
                table: "Videos",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_SessionId",
                table: "Videos",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_TeacherId",
                table: "Videos",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_Type_IsApproved",
                table: "Videos",
                columns: new[] { "Type", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoTags_TagId",
                table: "VideoTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTags_VideoId_TagId",
                table: "VideoTags",
                columns: new[] { "VideoId", "TagId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "QuestionTags");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SessionPurchases");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "VideoTags");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "ExplanationSessions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
