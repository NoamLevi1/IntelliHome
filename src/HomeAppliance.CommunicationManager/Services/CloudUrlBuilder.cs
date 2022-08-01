using IntelliHome.Common;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface ICloudUrlBuilder
{
    Uri GetCommunicationResponseReceiverUri();
    Uri GetCommunicationRequestSenderUri();
}

public sealed class CloudUrlBuilder : ICloudUrlBuilder
{
    private readonly Uri _baseUri;

    public CloudUrlBuilder(IConfigurationManager configurationManager, IIdGenerator idGenerator)
    {
        var serverUri = configurationManager.Get<WebApplicationConfiguration>().ServerUrl;

        _baseUri =
            new UriBuilder
            {
                Scheme = serverUri.Scheme,
                Port = serverUri.Port,
                Host = $"{idGenerator.GetOrCreateAsync().Await()}.{serverUri.Host}"
            }.Uri;
    }

    public Uri GetCommunicationResponseReceiverUri() =>
        new UriBuilder(_baseUri)
        {
            Path = "Api/CommunicationResponseReceiver"
        }.Uri;

    public Uri GetCommunicationRequestSenderUri() =>
        new UriBuilder(_baseUri)
        {
            Path = "Api/CommunicationRequestSender"
        }.Uri;
}