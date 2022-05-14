using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface ICloudUrlBuilder
{
    Uri GetCommunicationResponseReceiverUri();
    Uri GetCommunicationRequestSenderUri();
}

public sealed class CloudUrlBuilder : ICloudUrlBuilder
{
    private readonly WebApplicationConfiguration _configuration;

    public CloudUrlBuilder(IConfigurationManager configurationManager) =>
        _configuration = configurationManager.Get<WebApplicationConfiguration>();

    public Uri GetCommunicationResponseReceiverUri() =>
        new UriBuilder(_configuration.ServerUrl)
        {
            Path = "Api/CommunicationResponseReceiver"
        }.Uri;

    public Uri GetCommunicationRequestSenderUri() =>
        new UriBuilder(_configuration.ServerUrl)
        {
            Path = "Api/CommunicationRequestSender"
        }.Uri;
}