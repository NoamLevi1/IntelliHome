using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IntelliHome.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public sealed class HomeApplianceTunneledHttpMessageHandlerTests
{
    private readonly Mock<ILogger<HomeApplianceTunneledHttpMessageHandler>> _loggerMock = new();

    [TestMethod]
    public void TestHubIsCalled()
    {
        var hubMock = new Mock<IHomeApplianceHttpRequestMessageSenderHub>();

        var request = new HttpRequestMessage();
        var homeApplianceTunneledHttpMessageHandler = new HomeApplianceTunneledHttpMessageHandler(_loggerMock.Object, hubMock.Object);
        _ = homeApplianceTunneledHttpMessageHandler.SendAsync(request, CancellationToken.None);

        hubMock.Verify(
            hub =>
                hub.SendRequestAsync(
                    GetSendHttpRequestRequestReference(request),
                    It.IsAny<CancellationToken>()));
    }

    [TestMethod]
    public async Task TestReturnRightResponse()
    {
        var hubMock = new Mock<IHomeApplianceHttpRequestMessageSenderHub>();

        var request = new HttpRequestMessage();
        var response = new HttpResponseMessage();

        var homeApplianceTunneledHttpMessageHandler = new HomeApplianceTunneledHttpMessageHandler(_loggerMock.Object, hubMock.Object);
        hubMock.
            Setup(
                hub =>
                    hub.SendRequestAsync(
                        GetSendHttpRequestRequestReference(request),
                        It.IsAny<CancellationToken>())).
            Returns<SendHttpRequestRequest, CancellationToken>(
                (httpRequestRequest, _) =>
                {
                    homeApplianceTunneledHttpMessageHandler.ReceiveHttpResponse(new ReceiveHttpResponseRequest(httpRequestRequest.Id, response));

                    return Task.CompletedTask;
                });

        Assert.IsTrue(ReferenceEquals(await homeApplianceTunneledHttpMessageHandler.SendAsync(request, CancellationToken.None), response));
    }

    [TestMethod]
    public void TestNonExistIdThrowsException()
    {
        var hubMock = new Mock<IHomeApplianceHttpRequestMessageSenderHub>();
        var homeApplianceTunneledHttpMessageHandler = new HomeApplianceTunneledHttpMessageHandler(_loggerMock.Object, hubMock.Object);

        Assert.ThrowsException<KeyNotFoundException>(
            () => homeApplianceTunneledHttpMessageHandler.ReceiveHttpResponse(new ReceiveHttpResponseRequest(Guid.Empty, new HttpResponseMessage())));
    }

    private SendHttpRequestRequest GetSendHttpRequestRequestReference(HttpRequestMessage httpRequestMessage) =>
        It.Is<SendHttpRequestRequest>(sendHttpRequestRequest => ReferenceEquals(sendHttpRequestRequest.Request, httpRequestMessage));
}