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
public sealed class CommunicationManagerTests
{
    private readonly Mock<ILogger<CommunicationManager>> _loggerMock = new();

    [TestMethod]
    public void TestSendingIsCalled()
    {
        var requestSenderMock = new Mock<ICommunicationRequestSender>();

        var request = new MockRequest();
        _ = new CommunicationManager(_loggerMock.Object, requestSenderMock.Object).
            SendAsync<MockRequest, MockResponse>(
                Guid.Empty,
                request,
                CancellationToken.None);

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

        Assert.AreSame(
            response,
            await GetConfiguredCommunicationManager(request, response).
                SendAsync<MockRequest, MockResponse>(
                    Guid.Empty,
                    request,
                    CancellationToken.None));
    }

    [TestMethod]
    public async Task TestFailedRequestThrowsException()
    {
        var request = new MockRequest();
        var response = new ExceptionResponse(new Exception())
        {
            RequestId = request.Id
        };

        await Assert.ThrowsExceptionAsync<Exception>(
            () => GetConfiguredCommunicationManager(request, response).
                SendAsync<MockRequest, MockResponse>(
                    Guid.Empty,
                    request,
                    CancellationToken.None));
    }

    [TestMethod]
    public void TestNonExistIdThrowsException()
    {
        var homeApplianceTunneledHttpMessageHandler = new CommunicationManager(_loggerMock.Object, new Mock<ICommunicationRequestSender>().Object);

        Assert.ThrowsException<KeyNotFoundException>(
            () => homeApplianceTunneledHttpMessageHandler.SetResponse(new MockResponse()));
    }

    [TestMethod]
    public async Task TestDuplicateRequestThrowsException()
    {
        var requestSenderMock = new Mock<ICommunicationRequestSender>();

        var request = new MockRequest();
        var communicationManager = new CommunicationManager(_loggerMock.Object, requestSenderMock.Object);

        _ = communicationManager.SendAsync<MockRequest, MockResponse>(
            Guid.Empty,
            request,
            CancellationToken.None);

        await Assert.ThrowsExceptionAsync<Exception>(
            () => communicationManager.SendAsync<MockRequest, MockResponse>(
                Guid.Empty,
                request,
                CancellationToken.None));
    }

    private ICommunicationManager GetConfiguredCommunicationManager(ICommunicationRequest request, ICommunicationResponse response)
    {
        var requestSenderMock = new Mock<ICommunicationRequestSender>();

        var communicationManager = new CommunicationManager(_loggerMock.Object, requestSenderMock.Object);
        requestSenderMock.
            Setup(GetSendRequestAsyncExpression(request)).
            Callback(() => communicationManager.SetResponse(response));

        return communicationManager;
    }

    private Expression<Func<ICommunicationRequestSender, Task>> GetSendRequestAsyncExpression(ICommunicationRequest request) =>
        sender =>
            sender.SendRequestAsync(
                It.IsAny<Guid>(),
                request,
                It.IsAny<CancellationToken>());
}