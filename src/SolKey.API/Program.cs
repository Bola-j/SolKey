using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using SolKey.API.Middleware;
using SolKey.Application.Interfaces;
using SolKey.Application.DTOs.Auth;
using SolKey.Application.DTOs.Payments;
using SolKey.Application.DTOs.Questions;
using SolKey.Application.DTOs.Sessions;
using SolKey.Application.DTOs.Videos;
using SolKey.Application.Validators;
using SolKey.Infrastructure.Identity;
using SolKey.Infrastructure.Persistence;
using SolKey.Infrastructure.Services;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
                options.Filters.Add<Euphoric.FluentValidation.AspNetCore.ValidationActionFilter>();
                options.Filters.Add<Euphoric.FluentValidation.AspNetCore.ValidationExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        builder.Services.AddScoped<IValidator<CreateSessionRequest>, CreateSessionRequestValidator>();
        builder.Services.AddScoped<IValidator<CreatePaymentRequest>, CreatePaymentRequestValidator>();
        builder.Services.AddScoped<IValidator<UploadVideoRequest>, UploadVideoRequestValidator>();
        builder.Services.AddScoped<IValidator<AskQuestionRequest>, AskQuestionRequestValidator>();

        // =========================
        // Database
        // =========================
        builder.Services.AddDbContext<SolKeyDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")));

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
                options.IncludeErrorDetails = true;
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        NameClaimType = JwtRegisteredClaimNames.Sub,
                        RoleClaimType = "role",

                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                    };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuth");

                        if (!context.Request.Headers.ContainsKey("Authorization"))
                        {
                            logger.LogWarning("Missing Authorization header.");
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuth");

                        logger.LogWarning(context.Exception, "JWT authentication failed.");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        if (context.AuthenticateFailure is null)
                        {
                            return Task.CompletedTask;
                        }

                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuth");

                        logger.LogWarning(context.AuthenticateFailure, "JWT challenge triggered.");
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        // =========================
        // Email
        // =========================
        var emailOptions =
            builder.Configuration
                .GetSection("Email")
                .Get<EmailOptions>() ?? new EmailOptions();

        builder.Services.AddSingleton(emailOptions);
        builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

        // =========================
        // Swagger
        // =========================
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "SolKey API", Version = "v1" });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            };

            options.AddSecurityDefinition("Bearer", scheme);

            var schemeReference = new OpenApiSecuritySchemeReference("Bearer", null, null);
            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                { schemeReference, new List<string>() }
            });
        });

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