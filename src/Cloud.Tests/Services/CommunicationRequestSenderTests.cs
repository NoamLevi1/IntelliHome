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
using MongoDB.Driver;
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
    private Mock<IDatabase> _databaseMock = null!;
    private Mock<IMongoCollection<HomeAppliance>> _homeAppliancesCollectionMock = null!;
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
    public async Task TestOnConnectedCallsDatabase()
    {
        await _communicationRequestSender.OnConnectedAsync();

        _databaseMock.VerifyGet(database => database.HomeAppliances);
        _homeAppliancesCollectionMock.Verify(
            collection => collection.UpdateOneAsync(
                It.IsAny<FilterDefinition<HomeAppliance>>(),
                It.IsAny<UpdateDefinition<HomeAppliance>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()));
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
        _homeAppliancesCollectionMock.
            Setup(
                collection => collection.FindAsync(
                    It.IsAny<FilterDefinition<HomeAppliance>>(),
                    It.IsAny<FindOptions<HomeAppliance, HomeAppliance>>(),
                    It.IsAny<CancellationToken>())).
            ReturnsAsync(
                new MockCursor(
                    new[]
                    {
                        new HomeAppliance(_clientId)
                        {
                            ConnectionId = _connectionId
                        }
                    }));

        await _communicationRequestSender.SendRequestAsync(_clientId, communicationRequest, CancellationToken.None);

        _databaseMock.VerifyAll();
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
        _databaseMock = new Mock<IDatabase>();
        _homeAppliancesCollectionMock = new Mock<IMongoCollection<HomeAppliance>>();

        _communicationRequestSender = new CommunicationRequestSender(
            _loggerMock.Object,
            _databaseMock.Object,
            _hubContextMock.Object);
        _communicationRequestSender.Context = new MockHubCallerContext();

        _hubContextMock.SetupGet(context => context.Clients).Returns(_hubClientsMock.Object);
        _hubClientsMock.Setup(clients => clients.Client(_connectionId)).Returns(_clientProxyMock.Object);
        _databaseMock.SetupGet(store => store.HomeAppliances).Returns(_homeAppliancesCollectionMock.Object);
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

    private sealed class MockCursor : IAsyncCursor<HomeAppliance>
    {
        private readonly IEnumerator<IEnumerable<HomeAppliance>> _enumerator;

        public IEnumerable<HomeAppliance> Current => _enumerator.Current;

        public MockCursor(IEnumerable<HomeAppliance> homeAppliances) =>
            _enumerator =
                new List<IEnumerable<HomeAppliance>>
                {
                    homeAppliances
                }.GetEnumerator();

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public bool MoveNext(CancellationToken cancellationToken = new CancellationToken()) => _enumerator.MoveNext();

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken = new CancellationToken()) => Task.FromResult(_enumerator.MoveNext());
    }
}