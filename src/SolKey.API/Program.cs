using Amazon.Runtime;
using Amazon.S3;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SolKey.API.Middleware;
using SolKey.Application.DTOs.Auth;
using SolKey.Application.DTOs.Payments;
using SolKey.Application.DTOs.Questions;
using SolKey.Application.DTOs.Sessions;
using SolKey.Application.DTOs.Videos;
using SolKey.Application.Interfaces;
using SolKey.Application.Validators;
using SolKey.Infrastructure.Identity;   
using SolKey.Infrastructure.Persistence;
using SolKey.Infrastructure.Services;
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

        // Validators
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
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        // =========================
        // JWT
        // =========================
        var jwtOptions = builder.Configuration
            .GetSection("Jwt")
            .Get<JwtOptions>() ?? new JwtOptions();

        builder.Services.AddSingleton(jwtOptions);
        builder.Services.AddSingleton<JwtTokenService>();

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,

                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authHeader = context.Request.Headers.Authorization.ToString();

                        if (!string.IsNullOrWhiteSpace(authHeader) &&
                            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader["Bearer ".Length..].Trim();
                        }

                        return Task.CompletedTask;
                    },

                    OnTokenValidated = context =>
                    {
                        
                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        return Task.CompletedTask;
                    },

                    OnChallenge = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        // =========================
        // Email
        // =========================
        var emailOptions = builder.Configuration
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
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SolKey API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your JWT token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
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

            return string.IsNullOrWhiteSpace(storageAccessKey) ||
                   string.IsNullOrWhiteSpace(storageSecretKey)
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
            var bucketName = builder.Configuration["Storage:BucketName"] ?? "";
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

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SolKey API v1");
            options.RoutePrefix = string.Empty;
        });

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<SessionTrackingMiddleware>();

        app.MapControllers();

        app.Run();
    }
}