using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SessionCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SolKeyDbContext>();
                var cutoff = DateTime.UtcNow.AddDays(-7);

                var inactiveSessions = await dbContext.UserSessions
                    .Where(s => s.LastSeenAt < cutoff && s.IsActive)
                    .ToListAsync(stoppingToken);

                foreach (var session in inactiveSessions)
                {
                    session.IsActive = false;
                    session.ModifiedAt = DateTime.UtcNow;
                    session.ModifiedBy = "system";
                }

                var expiredTokens = await dbContext.RefreshTokens
                    .Where(t => t.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var token in expiredTokens)
                {
                    token.IsRevoked = true;
                    token.ModifiedAt = DateTime.UtcNow;
                    token.ModifiedBy = "system";
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
