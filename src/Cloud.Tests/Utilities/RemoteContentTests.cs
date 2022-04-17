using System;
using System.Threading;
using System.Threading.Tasks;
using IntelliHome.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public class RemoteContentTests
{
    [TestMethod]
    public async Task TestCreateContentReadStreamRequestSent()
    {
        var communicationClientMock = new Mock<ICommunicationClient>();
        var remoteContent = new RemoteContent(Guid.Empty, communicationClientMock.Object);

        communicationClientMock.
            Setup(
                client => client.SendAsync<CreateContentReadStreamRemoteContentRequest, CreateContentReadStreamRemoteContentResponse>(
                    It.Is<CreateContentReadStreamRemoteContentRequest>(request => request.ContentId == Guid.Empty),
                    It.IsAny<CancellationToken>())).
            ReturnsAsync(new CreateContentReadStreamRemoteContentResponse(Guid.Empty));

        await remoteContent.ReadAsStreamAsync(CancellationToken.None);

        communicationClientMock.VerifyAll();
    }

    [TestMethod]
    public void TestDisposeStreamRequestSent()
    {
        var communicationClientMock = new Mock<ICommunicationClient>();
        var remoteContent = new RemoteContent(Guid.Empty, communicationClientMock.Object);

        remoteContent.Dispose();

        communicationClientMock.Verify(
            client => client.SendAsync(
                It.Is<DisposeRemoteContentRequest>(request => request.ContentId == Guid.Empty),
                CancellationToken.None));
    }
}