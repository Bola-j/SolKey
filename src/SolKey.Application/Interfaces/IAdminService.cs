namespace SolKey.Application.Interfaces;

public interface IAdminService
{
    Task ApproveTeacherAsync(Guid teacherId, CancellationToken cancellationToken);
    Task ApproveVideoAsync(Guid videoId, CancellationToken cancellationToken);
    Task ApprovePaymentAsync(Guid paymentId, CancellationToken cancellationToken);
}
