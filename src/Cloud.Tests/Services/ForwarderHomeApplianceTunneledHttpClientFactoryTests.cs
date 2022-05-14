using System;
using System.Collections.Generic;
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
        var communicationManagerMock = new Mock<ICommunicationManager>();
        var forwarderHomeApplianceTunneledHttpClientFactory = new ForwarderHomeApplianceTunneledHttpClientFactory(communicationManagerMock.Object);

        var request = new HttpRequestMessage();
        var response = new HttpResponseMessage();

        communicationManagerMock.
            Setup(
                client =>
                    client.SendAsync<SendHomeAssistantHttpRequestRequest, SendHomeAssistantHttpRequestResponse>(
                        It.IsAny<Guid>(),
                        It.IsAny<SendHomeAssistantHttpRequestRequest>(),
                        CancellationToken.None)).
            ReturnsAsync(
                new SendHomeAssistantHttpRequestResponse(
                    new HttpResponseData
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

        await forwarderHomeApplianceTunneledHttpClientFactory.
            CreateClient(
                new ForwarderHttpClientContext
                {
                    NewConfig = new HttpClientConfig(),
                    NewMetadata = new Dictionary<string, string>
                    {
                        [ForwarderMetadataKey.HomeApplianceId] = Guid.Empty.ToString()
                    }
                }).
            SendAsync(request, CancellationToken.None);

        communicationManagerMock.VerifyAll();
    }
}