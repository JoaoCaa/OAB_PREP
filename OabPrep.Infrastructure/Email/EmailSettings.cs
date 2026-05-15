namespace OabPrep.Infrastructure.Email;

public enum EmailProvider { Smtp, SendGrid }

public sealed class EmailSettings
{
    public EmailProvider Provider { get; set; } = EmailProvider.Smtp;
    public string From { get; set; } = "noreply@oabprep.app";
    public string FromName { get; set; } = "OAB Prep";
    public string AppBaseUrl { get; set; } = "https://oabprep.app";
    public SmtpEmailSettings Smtp { get; set; } = new();
    public SendGridEmailSettings SendGrid { get; set; } = new();
}

public sealed class SmtpEmailSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;
}

public sealed class SendGridEmailSettings
{
    public string ApiKey { get; set; } = string.Empty;
}
