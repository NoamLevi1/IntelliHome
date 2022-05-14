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
    private const string _connectionId = nameof(_connectionId);
    private readonly Guid _clientId = Guid.Empty;

    private Mock<ILogger<CommunicationRequestSender>> _loggerMock = null!;
    private Mock<IHubContext<CommunicationRequestSender>> _hubContextMock = null!;
    private Mock<IHubClients> _hubClientsMock = null!;
    private Mock<IClientProxy> _clientProxyMock = null!;
    private Mock<IHomeApplianceStore> _clientStoreMock = null!;
    private CommunicationRequestSender _communicationRequestSender = null!;

    [TestMethod]
    public async Task TestOnConnectedCallsLogger()
    {
        await _communicationRequestSender.OnConnectedAsync();

        _loggerMock.Verify(
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
        await _communicationRequestSender.OnConnectedAsync();

        _clientStoreMock.Verify(store => store.AddOrUpdateHomeAppliance(_clientId,_connectionId));
    }

    [TestMethod]
    public async Task TestOnDisconnectedCallsLogger()
    {
        await _communicationRequestSender.OnDisconnectedAsync(null);

        _loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>) It.IsAny<object>())!));
    }

    [TestMethod]
    public async Task TestSendAsyncCallsContext()
    {
        var communicationRequest = new MockRequest();
        await _communicationRequestSender.SendRequestAsync(_clientId, communicationRequest, CancellationToken.None);

        _clientStoreMock.VerifyAll();
        _hubContextMock.VerifyAll();
        _hubClientsMock.VerifyAll();

        _clientProxyMock.Verify(
            proxy => proxy.SendCoreAsync(
                SignalRMethods.ReceiveRequest,
                new object?[]
                {
                    communicationRequest
                },
                CancellationToken.None));
    }

    [TestInitialize]
    public void Initialize()
    {
        _loggerMock = new Mock<ILogger<CommunicationRequestSender>>();
        _hubContextMock = new Mock<IHubContext<CommunicationRequestSender>>();
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        _clientStoreMock = new Mock<IHomeApplianceStore>();

        _communicationRequestSender = new CommunicationRequestSender(_loggerMock.Object, _hubContextMock.Object, _clientStoreMock.Object);
        _communicationRequestSender.Context = new MockHubCallerContext();

        _hubContextMock.SetupGet(context => context.Clients).Returns(_hubClientsMock.Object);
        _hubClientsMock.Setup(clients => clients.Client(_connectionId)).Returns(_clientProxyMock.Object);
        _clientStoreMock.Setup(store => store.GetConnectionId(_clientId)).Returns(_connectionId);
    }

    private sealed class MockHubCallerContext : HubCallerContext
    {
        public override string ConnectionId => _connectionId;
        public override string? UserIdentifier => null;
        public override ClaimsPrincipal? User => null;
        public override IDictionary<object, object?> Items => new Dictionary<object, object?>();
        public override IFeatureCollection Features => new Mock<IFeatureCollection>().Object;
        public override CancellationToken ConnectionAborted => CancellationToken.None;

        public override void Abort()
        {
        }
    }
}