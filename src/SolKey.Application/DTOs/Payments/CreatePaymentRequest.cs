namespace SolKey.Application.DTOs.Payments;

public record CreatePaymentRequest(decimal Amount, string Type, string ScreenshotPath, Guid? SessionId = null);
