using IntelliHome.Common;
using JetBrains.Annotations;

namespace IntelliHome.Cloud;

public sealed class CloudConfiguration : CommonConfiguration
{
    public DatabaseConfiguration DatabaseConfiguration { get; }
    public EmailSenderConfiguration? EmailSenderConfiguration { get; }

    [UsedImplicitly]
    public CloudConfiguration(IConfiguration configuration)
        : base(configuration)
    {
        DatabaseConfiguration = new DatabaseConfiguration(configuration.GetSection(nameof(DatabaseConfiguration)));
        var emailSenderConfigurationSection = configuration.GetSection(nameof(EmailSenderConfiguration));
        if (emailSenderConfigurationSection is not null)
        {
            EmailSenderConfiguration = new EmailSenderConfiguration(emailSenderConfigurationSection);
        }
    }
}