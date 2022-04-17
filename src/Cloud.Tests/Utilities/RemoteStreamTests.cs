using IntelliHome.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliHome.Cloud.Tests;

[TestClass]
public class RemoteStreamTests
{
    [TestMethod]
    public async Task TestDisposeAsyncCallsDisposeRequest() =>
        await TestAsync<RemoteStreamDisposeRequest>(async stream => await stream.DisposeAsync());

    [TestMethod]
    public void TestDisposeCallsDisposeRequest() =>
        Test<RemoteStreamDisposeRequest>(stream => stream.Dispose());

    [TestMethod]
    public async Task TestWriteAsyncCallsWriteRequest()
    {
        var buffer = new ReadOnlyMemory<byte>(new byte[10]);
        await TestAsync<RemoteStreamWriteRequest>(
            async stream => await stream.WriteAsync(buffer),
            request => request.Buffer.ToArray().SequenceEqual(buffer.ToArray()));
    }

    [TestMethod]
    public async Task TestReadAsyncCallsReadRequest()
    {
        var buffer = new Memory<byte>(new byte[10]);

        var response = new RemoteStreamReadResponse(buffer, buffer.Length);
        await TestAsync<RemoteStreamReadRequest, RemoteStreamReadResponse, int>(
            response,
            async stream => await stream.ReadAsync(buffer),
            num => Assert.AreEqual(response.Result, num),
            request => request.Count == buffer.Length);
    }

    [TestMethod]
    public async Task TestFlushAsyncCallsFlushRequest()
    {
        await TestAsync<RemoteStreamFlushRequest>(stream => stream.FlushAsync());
    }

    [TestMethod]
    public void TestFlushCallsFlushRequest()
    {
        Test<RemoteStreamFlushRequest>(stream => stream.Flush());
    }

    [TestMethod]
    [DataRow(850)]
    [DataRow(590)]
    [DataRow(74)]
    [DataRow(299)]
    [DataRow(345)]
    [DataRow(417)]
    [DataRow(777)]
    [DataRow(450)]
    [DataRow(674)]
    [DataRow(413)]
    [DataRow(259)]
    [DataRow(755)]
    [DataRow(106)]
    [DataRow(428)]
    [DataRow(296)]
    [DataRow(842)]
    [DataRow(621)]
    [DataRow(104)]
    [DataRow(788)]
    [DataRow(991)]
    [DataRow(223)]
    public void TestSetLengthCallsSetLengthRequest(long length)
    {
        Test<RemoteStreamSetLengthRequest>(stream => stream.SetLength(length), request => request.Value == length);
    }

    [TestMethod]
    [DataRow(799, SeekOrigin.Begin, 387)]
    [DataRow(548, SeekOrigin.Current, 455)]
    [DataRow(607, SeekOrigin.Current, 377)]
    [DataRow(908, SeekOrigin.Begin, 137)]
    [DataRow(514, SeekOrigin.End, 335)]
    [DataRow(706, SeekOrigin.End, 337)]
    [DataRow(733, SeekOrigin.Begin, 852)]
    [DataRow(38, SeekOrigin.Current, 954)]
    [DataRow(153, SeekOrigin.Begin, 564)]
    [DataRow(323, SeekOrigin.Begin, 749)]
    [DataRow(752, SeekOrigin.End, 536)]
    [DataRow(379, SeekOrigin.Begin, 887)]
    [DataRow(765, SeekOrigin.Begin, 896)]
    [DataRow(102, SeekOrigin.End, 814)]
    [DataRow(505, SeekOrigin.Current, 122)]
    [DataRow(128, SeekOrigin.Begin, 621)]
    [DataRow(592, SeekOrigin.Current, 409)]
    [DataRow(406, SeekOrigin.End, 145)]
    [DataRow(144, SeekOrigin.Current, 816)]
    [DataRow(398, SeekOrigin.Begin, 544)]
    [DataRow(244, SeekOrigin.Current, 329)]
    public void TestSeekCallsSeekRequest(long position, SeekOrigin origin, long result)
    {
        Test<RemoteStreamSeekRequest, RemoteStreamSeekResponse, long>(
            new RemoteStreamSeekResponse(result),
            stream => stream.Seek(position, origin),
            num => Assert.AreEqual(result, num),
            request => request.Offset == position && request.Origin == origin);
    }

    [TestMethod]
    [DataRow(850)]
    [DataRow(590)]
    [DataRow(74)]
    [DataRow(299)]
    [DataRow(345)]
    [DataRow(417)]
    [DataRow(777)]
    [DataRow(450)]
    [DataRow(674)]
    [DataRow(413)]
    [DataRow(259)]
    [DataRow(755)]
    [DataRow(106)]
    [DataRow(428)]
    [DataRow(296)]
    [DataRow(842)]
    [DataRow(621)]
    [DataRow(104)]
    [DataRow(788)]
    [DataRow(991)]
    [DataRow(223)]
    public void TestSetPositionCallsSetPositionRequest(long value)
    {
        Test<RemoteStreamSetPositionRequest>(stream => stream.Position = value, request => request.Value == value);
    }

    private void Test<TRequest>(Action<RemoteStream> testAction, Func<TRequest, bool>? additionalPredicate = null)
        where TRequest : RemoteStreamRequest, IVoidRequest
    {
        var communicationClientMock = new Mock<ICommunicationClient>();
        var remoteStream = new RemoteStream(Guid.Empty, communicationClientMock.Object);

        communicationClientMock.
            Setup(client => client.SendAsync(GetRequest(additionalPredicate), It.IsAny<CancellationToken>()));

        testAction(remoteStream);

        communicationClientMock.VerifyAll();
    }

    private async Task TestAsync<TRequest>(Func<RemoteStream, Task> testActionAsync, Func<TRequest, bool>? additionalPredicate = null)
        where TRequest : RemoteStreamRequest, IVoidRequest
    {
        var communicationClientMock = new Mock<ICommunicationClient>();
        var remoteStream = new RemoteStream(Guid.Empty, communicationClientMock.Object);

        communicationClientMock.
            Setup(
                client => client.SendAsync(
                    GetRequest(additionalPredicate),
                    It.IsAny<CancellationToken>()));

        await testActionAsync(remoteStream);

        communicationClientMock.VerifyAll();
    }

    private void Test<TRequest, TResponse, TActionResult>(TResponse response, Func<RemoteStream, TActionResult> testAction, Action<TActionResult> actionResultVerifier, Func<TRequest, bool>? additionalPredicate = null)
        where TResponse : ICommunicationResponse
        where TRequest : RemoteStreamRequest, IRequestWithResponse<TResponse>
    {
        var communicationClientMock = new Mock<ICommunicationClient>();
        var remoteStream = new RemoteStream(Guid.Empty, communicationClientMock.Object);

        communicationClientMock.
            Setup(client => client.SendAsync<TRequest, TResponse>(GetRequest(additionalPredicate), It.IsAny<CancellationToken>())).
            ReturnsAsync(response);

        var actionResult = testAction(remoteStream);

        communicationClientMock.VerifyAll();
        actionResultVerifier(actionResult);
    }

    private async Task TestAsync<TRequest, TResponse, TActionResult>(TResponse response, Func<RemoteStream, Task<TActionResult>> testActionAsync, Action<TActionResult> actionResultVerifier, Func<TRequest, bool>? additionalPredicate = null)
        where TResponse : ICommunicationResponse
        where TRequest : RemoteStreamRequest, IRequestWithResponse<TResponse>
    {
        var communicationClientMock = new Mock<ICommunicationClient>();
        var remoteStream = new RemoteStream(Guid.Empty, communicationClientMock.Object);

        communicationClientMock.
            Setup(
                client =>
                    client.SendAsync<TRequest, TResponse>(
                        GetRequest(additionalPredicate),
                        It.IsAny<CancellationToken>())).
            ReturnsAsync(response);

        var actionResult = await testActionAsync(remoteStream);

        communicationClientMock.VerifyAll();
        actionResultVerifier(actionResult);
    }

    private TRequest GetRequest<TRequest>(Func<TRequest, bool>? additionalPredicate)
        where TRequest : RemoteStreamRequest =>
        It.Is<TRequest>(request => request.StreamId == Guid.Empty && (additionalPredicate == null || additionalPredicate(request)));
}