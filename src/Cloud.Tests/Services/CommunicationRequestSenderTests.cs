using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IntelliHome.Common;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public sealed class CommunicationRequestSenderTests
{
    [TestMethod]
    public async Task TestOnConnectedCallsLogger()
    {
        var loggerMock = new Mock<ILogger<CommunicationRequestSender>>();
        var hubContextMock = new Mock<IHubContext<CommunicationRequestSender>>();
        var clientStoreMock = new Mock<IClientStore>();

        var communicationRequestSender = new CommunicationRequestSender(loggerMock.Object, hubContextMock.Object, clientStoreMock.Object);
        clientStoreMock.Setup(store => store.Clients).Returns(new ConcurrentHashSet<string>());

        communicationRequestSender.Context = new MockHubCallerContext("");

        await communicationRequestSender.OnConnectedAsync();

        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>) It.IsAny<object>())!));
    }

    [TestMethod]
    public async Task TestOnConnectedCallsStore()
    {
        var loggerMock = new Mock<ILogger<CommunicationRequestSender>>();
        var hubContextMock = new Mock<IHubContext<CommunicationRequestSender>>();
        var clientStoreMock = new Mock<IClientStore>();

        var communicationRequestSender = new CommunicationRequestSender(loggerMock.Object, hubContextMock.Object, clientStoreMock.Object);
        clientStoreMock.Setup(store => store.Clients).Returns(new ConcurrentHashSet<string>());

        communicationRequestSender.Context = new MockHubCallerContext("");

        await communicationRequestSender.OnConnectedAsync();

        clientStoreMock.VerifyGet(store => store.Clients);
    }

    [TestMethod]
    public async Task TestOnDisconnectedCallsLogger()
    {
        var loggerMock = new Mock<ILogger<CommunicationRequestSender>>();
        var hubContextMock = new Mock<IHubContext<CommunicationRequestSender>>();
        var clientStoreMock = new Mock<IClientStore>();

        var communicationRequestSender = new CommunicationRequestSender(loggerMock.Object, hubContextMock.Object, clientStoreMock.Object);
        clientStoreMock.Setup(store => store.Clients).Returns(new ConcurrentHashSet<string>());

        communicationRequestSender.Context = new MockHubCallerContext("");

        await communicationRequestSender.OnDisconnectedAsync(null);

        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>) It.IsAny<object>())!));
    }

    [TestMethod]
    public async Task TestOnDisconnectedCallsStore()
    {
        var loggerMock = new Mock<ILogger<CommunicationRequestSender>>();
        var hubContextMock = new Mock<IHubContext<CommunicationRequestSender>>();
        var clientStoreMock = new Mock<IClientStore>();

        var communicationRequestSender = new CommunicationRequestSender(loggerMock.Object, hubContextMock.Object, clientStoreMock.Object);
        clientStoreMock.Setup(store => store.Clients).Returns(new ConcurrentHashSet<string>());

        communicationRequestSender.Context = new MockHubCallerContext("");

        await communicationRequestSender.OnDisconnectedAsync(null);

        clientStoreMock.VerifyGet(store => store.Clients);
    }

    [TestMethod]
    public async Task TestSendAsyncCallsContext()
    {
        var loggerMock = new Mock<ILogger<CommunicationRequestSender>>();
        var hubContextMock = new Mock<IHubContext<CommunicationRequestSender>>();
        var hubClientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        var clientStoreMock = new Mock<IClientStore>();

        var communicationRequestSender = new CommunicationRequestSender(loggerMock.Object, hubContextMock.Object, clientStoreMock.Object);
        clientStoreMock.Setup(store => store.Clients).Returns(new ConcurrentHashSet<string> {"Client"});

        hubContextMock.SetupGet(context => context.Clients).Returns(hubClientsMock.Object);
        hubClientsMock.Setup(clients => clients.Client("Client")).Returns(clientProxyMock.Object);

        var communicationRequest = new MockRequest();
        await communicationRequestSender.SendRequestAsync(communicationRequest, CancellationToken.None);

        clientStoreMock.VerifyAll();
        hubContextMock.VerifyAll();
        hubClientsMock.VerifyAll();

        clientProxyMock.Verify(
            proxy => proxy.SendCoreAsync(
                SignalRMethods.ReceiveRequest,
                new object?[]
                {
                    communicationRequest
                },
                CancellationToken.None));
    }

    private sealed class MockHubCallerContext : HubCallerContext
    {
        public override string ConnectionId { get; }
        public override string? UserIdentifier => null;
        public override ClaimsPrincipal? User => null;
        public override IDictionary<object, object?> Items => new Dictionary<object, object?>();
        public override IFeatureCollection Features => new Mock<IFeatureCollection>().Object;
        public override CancellationToken ConnectionAborted => CancellationToken.None;

        public MockHubCallerContext(string connectionId) =>
            ConnectionId = connectionId;

        public override void Abort()
        {
        }
    }
}