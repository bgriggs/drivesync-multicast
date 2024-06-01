using System.Net.Sockets;

namespace BigMission.RedMist.DriveSync.Multicast.Tests;

internal class TestSocketFactory : ISocketFactory
{
    public TestSocket Socket { get; set; } = new TestSocket();

    public ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
    {
        return Socket;
    }
}
