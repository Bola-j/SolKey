using Microsoft.EntityFrameworkCore;
using SolKey.Application.Interfaces;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly SolKeyDbContext _dbContext;

    public AdminService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ApproveTeacherAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == teacherId, cancellationToken)
            ?? throw new InvalidOperationException("Teacher not found.");

        teacher.IsVerifiedTeacher = true;
        teacher.ModifiedAt = DateTime.UtcNow;
        teacher.ModifiedBy = "admin";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveVideoAsync(Guid videoId, CancellationToken cancellationToken)
    {
        var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        video.IsApproved = true;
        video.ModifiedAt = DateTime.UtcNow;
        video.ModifiedBy = "admin";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApprovePaymentAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException("Payment not found.");

        payment.IsApproved = true;
        payment.ModifiedAt = DateTime.UtcNow;
        payment.ModifiedBy = "admin";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
