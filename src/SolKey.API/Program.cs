using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Euphoric.FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SolKey.API.Middleware;
using SolKey.Application.Interfaces;
using SolKey.Infrastructure.Identity;
using SolKey.Infrastructure.Persistence;
using SolKey.Infrastructure.Services;
using System.Text;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // =========================
        // Logging
        // =========================
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();

        // =========================
        // Controllers
        // =========================
        builder.Services
            .AddControllers(options =>
            {
                options.Filters.Add<ValidationActionFilter>();
                options.Filters.Add<ValidationExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        // =========================
        // Database
        // =========================
        builder.Services.AddDbContext<SolKeyDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("SolKeyDatabase")));

        // =========================
        // JWT
        // =========================
        var jwtOptions =
            builder.Configuration
                .GetSection("Jwt")
                .Get<JwtOptions>() ?? new JwtOptions();

        builder.Services.AddSingleton(jwtOptions);
        builder.Services.AddSingleton<JwtTokenService>();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                    };
            });

        builder.Services.AddAuthorization();

        // =========================
        // Swagger
        // =========================
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        // =========================
        // AWS S3
        // =========================
        var storageAccessKey = builder.Configuration["Storage:AccessKey"];
        var storageSecretKey = builder.Configuration["Storage:SecretKey"];
        var storageServiceUrl = builder.Configuration["Storage:ServiceUrl"];

        builder.Services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = storageServiceUrl,
                ForcePathStyle = true
            };

            return string.IsNullOrWhiteSpace(storageAccessKey)
                || string.IsNullOrWhiteSpace(storageSecretKey)
                ? new AmazonS3Client(config)
                : new AmazonS3Client(
                    new BasicAWSCredentials(storageAccessKey, storageSecretKey),
                    config);
        });

        // =========================
        // Application Services
        // =========================
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IBookService, BookService>();
        builder.Services.AddScoped<IChapterService, ChapterService>();
        builder.Services.AddScoped<ILessonService, LessonService>();
        builder.Services.AddScoped<IQuestionService, QuestionService>();
        builder.Services.AddScoped<IVideoService, VideoService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<IAccessControlService, AccessControlService>();

        builder.Services.AddScoped<IStorageService>(provider =>
        {
            var s3Client = provider.GetRequiredService<IAmazonS3>();

            var bucketName =
                builder.Configuration["Storage:BucketName"]
                ?? string.Empty;

            return new StorageService(s3Client, bucketName);
        });

        // =========================
        // Background Services
        // =========================
        builder.Services.AddHostedService<SessionCleanupService>();

        // =========================
        // Build App
        // =========================
        var app = builder.Build();

        // =========================
        // Middleware
        // =========================
        app.UseSerilogRequestLogging();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Swagger
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "SolKey API v1");

            options.RoutePrefix = string.Empty;
        });

        // HTTPS
        app.UseHttpsRedirection();

        // Auth
        app.UseAuthentication();
        app.UseAuthorization();

        // Custom Middleware
        app.UseMiddleware<SessionTrackingMiddleware>();

        // Controllers
        app.MapControllers();

        app.Run();
    }
}