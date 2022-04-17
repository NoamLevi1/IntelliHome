using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IntelliHome.Common;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public sealed class CommunicationClientTests
{
    private readonly Mock<ILogger<CommunicationClient>> _loggerMock = new();

    [TestMethod]
    public void TestSendingIsCalled()
    {
        var requestSenderMock = new Mock<ICommunicationRequestSender>();

        var request = new MockRequest();
        _ = new CommunicationClient(_loggerMock.Object, requestSenderMock.Object).
            SendAsync<MockRequest, MockResponse>(request, CancellationToken.None);

        requestSenderMock.Verify(GetSendRequestAsyncExpression(request));
    }

    [TestMethod]
    public async Task TestReturnRightResponse()
    {
        var request = new MockRequest();
        var response = new MockResponse
        {
            RequestId = request.Id
        };

        Assert.AreSame(response, await GetConfiguredCommunicationClient(request, response).SendAsync<MockRequest, MockResponse>(request, CancellationToken.None));
    }

    [TestMethod]
    public async Task TestVoidRequestReturns()
    {
        var request = new MockVoidRequest();
        var response = new VoidResponse
        {
            RequestId = request.Id
        };

        await GetConfiguredCommunicationClient(request, response).SendAsync(request, CancellationToken.None);
    }

    [TestMethod]
    public async Task TestFailedRequestThrowsException()
    {
        var request = new MockVoidRequest();
        var response = new ExceptionResponse(new Exception())
        {
            RequestId = request.Id
        };

        await Assert.ThrowsExceptionAsync<Exception>(() => GetConfiguredCommunicationClient(request, response).SendAsync(request, CancellationToken.None));
    }

    [TestMethod]
    public void TestNonExistIdThrowsException()
    {
        var homeApplianceTunneledHttpMessageHandler = new CommunicationClient(_loggerMock.Object, new Mock<ICommunicationRequestSender>().Object);

        Assert.ThrowsException<KeyNotFoundException>(
            () => homeApplianceTunneledHttpMessageHandler.SetResponse(new MockResponse()));
    }

    [TestMethod]
    public async Task TestDuplicateRequestThrowsException()
    {
        var requestSenderMock = new Mock<ICommunicationRequestSender>();

        var request = new MockRequest();
        var communicationClient = new CommunicationClient(_loggerMock.Object, requestSenderMock.Object);

        _ = communicationClient.SendAsync<MockRequest, MockResponse>(request, CancellationToken.None);

        await Assert.ThrowsExceptionAsync<Exception>(() => communicationClient.SendAsync<MockRequest, MockResponse>(request, CancellationToken.None));
    }

    private ICommunicationClient GetConfiguredCommunicationClient(ICommunicationRequest request, ICommunicationResponse response)
    {
        var requestSenderMock = new Mock<ICommunicationRequestSender>();

        var communicationClient = new CommunicationClient(_loggerMock.Object, requestSenderMock.Object);
        requestSenderMock.
            Setup(GetSendRequestAsyncExpression(request)).
            Callback(() => communicationClient.SetResponse(response));

        return communicationClient;
    }

    private Expression<Func<ICommunicationRequestSender, Task>> GetSendRequestAsyncExpression(ICommunicationRequest request) =>
        sender =>
            sender.SendRequestAsync(
                request,
                It.IsAny<CancellationToken>());
}