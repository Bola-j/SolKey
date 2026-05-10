using Microsoft.EntityFrameworkCore;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class SessionTrackingMiddleware
{
    private readonly RequestDelegate _next;

    public SessionTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, SolKeyDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst("sub")?.Value;
            var deviceId = context.Request.Headers["X-Device-Id"].ToString();

            if (Guid.TryParse(userIdClaim, out var userId) && !string.IsNullOrWhiteSpace(deviceId))
            {
                var session = await dbContext.UserSessions.FirstOrDefaultAsync(s => s.UserId == userId && s.DeviceId == deviceId && s.IsActive);
                if (session is not null)
                {
                    session.LastSeenAt = DateTime.UtcNow;
                    session.ModifiedAt = DateTime.UtcNow;
                    session.ModifiedBy = userId.ToString();
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        await _next(context);
    }
}
