using IntelliHome.Common;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IntelliHome.Cloud;

public interface IEmailSender
{
    Task SendAsync(EmailMessage emailMessage);
}

public sealed class EmailSender : IEmailSender
{
    private static readonly EmailAddress _sourceAddress = new("intellihomereset@gmail.com", "IntelliHome Password Reset");

    private readonly ILogger<EmailSender> _logger;
    private readonly SendGridClient _client;

    public EmailSender(ILogger<EmailSender> logger, IConfigurationManager configurationManager)
    {
        _logger = logger;
        _client = new SendGridClient(configurationManager.Get<EmailSenderConfiguration>().ApiKey);
    }

    public async Task SendAsync(EmailMessage emailMessage)
    {
        _logger.LogInformation($"SendAsync started [{nameof(emailMessage.To)}={emailMessage.To} {nameof(emailMessage.Subject)}={emailMessage.Subject}]");

        var sendGridMessage = new SendGridMessage
        {
            From = _sourceAddress,
            Subject = emailMessage.Subject,
            PlainTextContent = emailMessage.Content
        };
        sendGridMessage.AddTo(emailMessage.To);

        await _client.SendEmailAsync(sendGridMessage);

        _logger.LogInformation($"SendAsync Finished [{nameof(emailMessage.To)}={emailMessage.To} {nameof(emailMessage.Subject)}={emailMessage.Subject}]");
    }
}

public sealed class DevelopmentEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger) =>
        _logger = logger;

    public Task SendAsync(EmailMessage emailMessage)
    {
        _logger.LogInformation(
            "Sending Message [" +
            $"{nameof(emailMessage.To)}={emailMessage.To} " +
            $"{nameof(emailMessage.Subject)}={emailMessage.Subject} " +
            $"{nameof(emailMessage.Content)}={emailMessage.Content}]");

        return Task.CompletedTask;
    }
}

public sealed class EmailSenderConfiguration : IServiceConfiguration
{
    public string ApiKey { get; }

    public EmailSenderConfiguration(IConfiguration configuration) =>
        ApiKey = configuration.GetValue<string>(nameof(ApiKey));
}

public sealed class EmailMessage
{
    public string To { get; }
    public string Subject { get; }
    public string Content { get; }

    public EmailMessage(
        string to,
        string subject,
        string content)
    {
        To = to;
        Subject = subject;
        Content = content;
    }
}