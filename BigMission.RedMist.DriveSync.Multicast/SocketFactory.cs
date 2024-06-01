using System.Net.Sockets;

namespace BigMission.RedMist.DriveSync.Multicast;

public interface ISocketFactory
{
    ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
}


public class SocketFactory : ISocketFactory
{
    public ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
    {
        var socket = new Socket(addressFamily, socketType, protocolType);
        var ws = new WrappedSocket(socket);

        return ws;
    }
}