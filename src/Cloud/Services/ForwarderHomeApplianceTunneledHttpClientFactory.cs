using Humanizer;
using IntelliHome.Common;
using Yarp.ReverseProxy.Forwarder;

namespace IntelliHome.Cloud
{
    public sealed class ForwarderHomeApplianceTunneledHttpClientFactory : ForwarderHttpClientFactory
    {
        private readonly ICommunicationManager _communicationManager;

        public ForwarderHomeApplianceTunneledHttpClientFactory(ICommunicationManager communicationManager) =>
            _communicationManager = communicationManager;

        protected override HttpMessageHandler WrapHandler(ForwarderHttpClientContext context, HttpMessageHandler handler)
        {
            Ensure.NotNull(context.NewMetadata);

            handler.Dispose();
            return new CustomHttpMessageHandler(
                new CommunicationClient(
                    Guid.Parse(context.NewMetadata[ForwarderMetadataKey.HomeApplianceId]),
                    _communicationManager));
        }

        private sealed class CustomHttpMessageHandler : HttpMessageHandler
        {
            private readonly ICommunicationClient _communicationClient;
            private readonly HttpResponseMessageBuilder _httpResponseMessageBuilder;

            public CustomHttpMessageHandler(ICommunicationClient communicationClient)
            {
                _communicationClient = communicationClient;
                _httpResponseMessageBuilder = new HttpResponseMessageBuilder(_communicationClient);
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                _httpResponseMessageBuilder.Build(
                    (await _communicationClient.SendAsync<SendHomeAssistantHttpRequestRequest, SendHomeAssistantHttpRequestResponse>(
                            new SendHomeAssistantHttpRequestRequest(await HttpRequestData.FromHttpRequestMessageAsync(request, cancellationToken)),
                            cancellationToken).
                        WaitAsync(3.Minutes(), cancellationToken)).
                    HttpResponseData);
        }
    }
}