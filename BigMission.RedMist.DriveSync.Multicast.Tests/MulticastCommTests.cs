using BigMission.TestHelpers.Testing;
using Moq;
using System.Net;
using System.Net.Sockets;

namespace BigMission.RedMist.DriveSync.Multicast.Tests;

[TestClass]
public class MulticastCommTests
{
    [TestMethod]
    public async Task SendAsync_ShouldThrowException_WhenPayloadIsEmpty()
    {
        var socketFactory = new Mock<ISocketFactory>();
        var socket = new Mock<ISocket>();
        var configurationProvider = new TestServerConfiguration();
        socketFactory.Setup(s => s.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.IsAny<ProtocolType>())).Returns(socket.Object);

        var multicastComm = new MulticastComm(configurationProvider, socketFactory.Object);

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => multicastComm.SendAsync([], 1));
    }

    [TestMethod]
    public async Task SendAsync_ShouldThrowException_WhenPayloadIsTooLarge()
    {
        var socketFactory = new Mock<ISocketFactory>();
        var socket = new Mock<ISocket>();
        var configurationProvider = new TestServerConfiguration();
        socketFactory.Setup(s => s.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.IsAny<ProtocolType>())).Returns(socket.Object);

        var multicastComm = new MulticastComm(configurationProvider, socketFactory.Object);

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => multicastComm.SendAsync(new byte[4097], 1));
    }

    [TestMethod]
    public async Task SendAsync_ShouldConnectAndSend_WhenSocketIsNotConnected()
    {
        var configurationProvider = new TestServerConfiguration();
        var socketFactory = new Mock<ISocketFactory>();
        var socket = new Mock<ISocket>();
        socket.Setup(s => s.Connected).Returns(false);
        socketFactory.Setup(s => s.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.IsAny<ProtocolType>())).Returns(socket.Object);

        var multicastComm = new MulticastComm(configurationProvider, socketFactory.Object);

        await multicastComm.SendAsync(new byte[1], 1);

        socket.Verify(s => s.ConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<CancellationToken>()), Times.Once);
        socket.Verify(s => s.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<SocketFlags>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task SendAsync_ShouldOnlySend_WhenSocketIsAlreadyConnected()
    {
        var socketFactory = new Mock<ISocketFactory>();
        var socket = new Mock<ISocket>();
        var configurationProvider = new TestServerConfiguration();
        socket.Setup(s => s.Connected).Returns(true);
        socketFactory.Setup(s => s.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.IsAny<ProtocolType>())).Returns(socket.Object);

        var multicastComm = new MulticastComm(configurationProvider, socketFactory.Object);

        await multicastComm.SendAsync(new byte[1], 1);

        socket.Verify(s => s.ConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<CancellationToken>()), Times.Never);
        socket.Verify(s => s.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<SocketFlags>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [TestMethod]
    public async Task ReceiveMessagesAsync_ShouldTriggerOnReceived_WhenMessageIsReceived()
    {
        var socketFactory = new TestSocketFactory();
        var loggerFactory = new TestLoggerFactory();
        var configurationProvider = new TestServerConfiguration();

        var buff = new byte[] { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };
        socketFactory.Socket.Bytes = buff;
        socketFactory.Socket.Count = buff.Length;

        var multicastComm = new MulticastComm(configurationProvider, socketFactory, loggerFactory);

        bool received = false;
        multicastComm.OnReceived += (payloadHeader) =>
        {
            received = true;
            return Task.CompletedTask;
        };

        var cts = new CancellationTokenSource();
        var _ = Task.Run(() => multicastComm.ReceiveMessagesAsync(cts.Token));

        await Task.Delay(1000); // Wait for a while to let the ReceiveMessagesAsync method run
        cts.Cancel(); // Stop the ReceiveMessagesAsync method

        Assert.IsTrue(received);
    }

    [TestMethod]
    public async Task ReceiveMessagesAsync_ShouldNotTriggerOnReceived_WhenMessageIsFromSelf()
    {
        var socketFactory = new TestSocketFactory();
        var loggerFactory = new TestLoggerFactory();
        var configurationProvider = new TestServerConfiguration();
        var id = configurationProvider.GetServerConfiguration().ApplicationInstance;

        var buff = new List<byte> { 1 };
        buff.AddRange(id.ToByteArray());
        buff.Add(0);
        socketFactory.Socket.Bytes = [.. buff];
        socketFactory.Socket.Count = buff.Count;

        var multicastComm = new MulticastComm(configurationProvider, socketFactory, loggerFactory);

        bool received = false;
        multicastComm.OnReceived += (payloadHeader) =>
        {
            received = true;
            return Task.CompletedTask;
        };

        var cts = new CancellationTokenSource();
        var _ = Task.Run(() => multicastComm.ReceiveMessagesAsync(cts.Token));

        await Task.Delay(1000); // Wait for a while to let the ReceiveMessagesAsync method run
        cts.Cancel(); // Stop the ReceiveMessagesAsync method

        Assert.IsFalse(received);
    }
}
