using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IntelliHome.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public class ForwarderHomeApplianceTunneledHttpClientFactoryTests
{
    [TestMethod]
    public async Task TestReturnsRightResponse()
    {
        var communicationClientMock = new Mock<ICommunicationClient>();
        var messageBuilderMock = new Mock<IHttpResponseMessageBuilder>();
        var forwarderHomeApplianceTunneledHttpClientFactory = new ForwarderHomeApplianceTunneledHttpClientFactory(communicationClientMock.Object, messageBuilderMock.Object);

        var request = new HttpRequestMessage();
        var response = new HttpResponseMessage();

        communicationClientMock.
            Setup(
                client =>
                    client.SendAsync<SendHomeAssistantHttpRequestRequest, SendHomeAssistantHttpRequestResponse>(
                        It.IsAny<SendHomeAssistantHttpRequestRequest>(),
                        CancellationToken.None)).
            ReturnsAsync(new SendHomeAssistantHttpRequestResponse(new HttpResponseData
            {
                ContentId = Guid.Empty,
                ContentHeaders = response.Content.Headers,
                Headers = response.Headers,
                ReasonPhrase = response.ReasonPhrase,
                RequestData = response.RequestMessage is null
                    ? null
                    : await HttpRequestData.FromHttpRequestMessageAsync(response.RequestMessage, CancellationToken.None),
                Version = response.Version,
                StatusCode = response.StatusCode
            }));
        messageBuilderMock.Setup(builder => builder.Build(It.IsAny<HttpResponseData>())).Returns(response);

        var result = await forwarderHomeApplianceTunneledHttpClientFactory.CreateClient(new ForwarderHttpClientContext{ NewConfig = new HttpClientConfig()}).SendAsync(request, CancellationToken.None);

        communicationClientMock.VerifyAll();
        messageBuilderMock.VerifyAll();

        Assert.AreSame(response, result);
    }
}