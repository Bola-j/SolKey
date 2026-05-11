using Amazon.S3;
using Euphoric.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SolKey.Application.Interfaces;
using SolKey.API.Middleware;
using SolKey.Infrastructure.Identity;
using SolKey.Infrastructure.Persistence;
using SolKey.Infrastructure.Services;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddFluentValidationClientsideAdapters();

        builder.Services.AddDbContext<SolKeyDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("SolKeyDatabase")));

        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        builder.Services.AddSingleton(jwtOptions);
        builder.Services.AddSingleton(new JwtTokenService(jwtOptions));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "SolKey API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
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
            Array.Empty<string>()
        }
            });
        });

        builder.Services.AddAWSService<IAmazonS3>(new Amazon.Extensions.NETCore.Setup.AWSOptions
        {
            ServiceURL = builder.Configuration["Storage:ServiceUrl"]
        });

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
            var bucketName = builder.Configuration["Storage:BucketName"] ?? string.Empty;
            return new StorageService(s3Client, bucketName);
        });

        builder.Services.AddHostedService<SessionCleanupService>();

        var app = builder.Build();

        app.UseSerilogRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<SessionTrackingMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
