using Yarp.ReverseProxy.Forwarder;

namespace IntelliHome.Cloud
{
    public sealed class ForwarderHomeApplianceTunneledHttpClientFactory : IForwarderHttpClientFactory
    {
        private readonly IHomeApplianceTunneledHttpMessageHandler _homeApplianceTunneledHttpMessageHandler;

        public ForwarderHomeApplianceTunneledHttpClientFactory(IHomeApplianceTunneledHttpMessageHandler homeApplianceTunneledHttpMessageHandler) =>
            _homeApplianceTunneledHttpMessageHandler = homeApplianceTunneledHttpMessageHandler;

        public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context) =>
            new(new CustomHttpMessageHandler(_homeApplianceTunneledHttpMessageHandler));

        private sealed class CustomHttpMessageHandler : HttpMessageHandler
        {
            private readonly IHomeApplianceTunneledHttpMessageHandler _homeApplianceTunneledHttpMessageHandler;

            public CustomHttpMessageHandler(IHomeApplianceTunneledHttpMessageHandler homeApplianceTunneledHomeApplianceTunneledHttpMessageHandler) =>
                _homeApplianceTunneledHttpMessageHandler = homeApplianceTunneledHomeApplianceTunneledHttpMessageHandler;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                _homeApplianceTunneledHttpMessageHandler.SendAsync(request, cancellationToken);
        }
    }
}