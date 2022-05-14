using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public class CommunicationResponseReceiverTests
{
    [TestMethod]
    public void TestClientIsCalledOnReceive()
    {
        var communicationClientMock = new Mock<ICommunicationManager>();
        var communicationResponseReceiver = new CommunicationResponseReceiver(communicationClientMock.Object);

        var response = new MockResponse();
        communicationResponseReceiver.ReceiveResponse(response);

        communicationClientMock.Verify(client => client.SetResponse(response));
    }
}