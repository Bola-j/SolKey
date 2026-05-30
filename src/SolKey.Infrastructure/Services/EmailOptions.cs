namespace SolKey.Infrastructure.Services;

public class EmailOptions
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string VerificationBaseUrl { get; set; } = string.Empty;
    public int TokenExpiryHours { get; set; } = 24;
    public int ResendCooldownMinutes { get; set; } = 60;
    public SmtpOptions Smtp { get; set; } = new();
}

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
}
