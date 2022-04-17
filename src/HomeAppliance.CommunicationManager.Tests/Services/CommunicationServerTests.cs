using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliHome.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IntelliHome.HomeAppliance.CommunicationManager.Tests;

[TestClass]
public class CommunicationServerTests
{
    private readonly Mock<ILogger<CommunicationServer>> _loggerMock = new();

    private Mock<ICommunicationHandler>? _communicationHandlerMock;
    private Mock<ICommunicationResponseSender>? _communicationResponseSenderMock;
    private CommunicationServer? _communicationServer;

    [TestMethod]
    public async Task TestCommunicationHandlerIsCalled()
    {
        var request = new Request();
        var response = new Response();

        _communicationHandlerMock!.Setup(handler => handler.HandleRequestAsync(request)).ReturnsAsync(response);

        await _communicationServer!.ServeRequestAsync(request, CancellationToken.None);

        _communicationHandlerMock.VerifyAll();
    }

    [TestMethod]
    public async Task TestCommunicationResponseSenderIsCalled()
    {
        var request = new Request();
        var response = new Response();

        _communicationHandlerMock!.Setup(handler => handler.HandleRequestAsync(request)).ReturnsAsync(response);

        await _communicationServer!.ServeRequestAsync(request, CancellationToken.None);

        _communicationResponseSenderMock!.Verify(sender => sender.SendResponseAsync(response, CancellationToken.None));
    }

    [TestMethod]
    public async Task TestResponseIsFilledWithRequestId()
    {
        var request = new Request();
        var response = new Response();

        _communicationHandlerMock!.Setup(handler => handler.HandleRequestAsync(request)).ReturnsAsync(response);

        await _communicationServer!.ServeRequestAsync(request, CancellationToken.None);

        Assert.AreEqual(request.Id, response.RequestId);
    }

    [TestMethod]
    public async Task TestExceptionResultInExceptionResponse()
    {
        var request = new Request();
        var exception = new TestException();

        _communicationHandlerMock!.Setup(handler => handler.HandleRequestAsync(request)).Throws(exception);

        await Assert.ThrowsExceptionAsync<TestException>(() => _communicationServer!.ServeRequestAsync(request, CancellationToken.None));
        _communicationResponseSenderMock!.Verify(
            sender =>
                sender.SendResponseAsync(
                    It.Is<ExceptionResponse>(response => response.RequestId == request.Id && ReferenceEquals(response.Exception, exception)),
                    CancellationToken.None));
    }

    [TestInitialize]
    public void Initialize()
    {
        _communicationHandlerMock = new Mock<ICommunicationHandler>();
        _communicationResponseSenderMock = new Mock<ICommunicationResponseSender>();
        _communicationServer = new CommunicationServer(_loggerMock.Object, _communicationHandlerMock.Object, _communicationResponseSenderMock.Object);
    }

    private class Request : CommunicationRequest
    {
    }

    private class Response : CommunicationResponse
    {
    }

    private class TestException : Exception
    {
    }
}