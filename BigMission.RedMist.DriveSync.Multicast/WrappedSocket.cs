using System.Net;
using System.Net.Sockets;

namespace BigMission.RedMist.DriveSync.Multicast;

public class WrappedSocket(Socket socket) : ISocket
{
    private Socket Socket { get; } = socket;

    public bool Connected
    {
        get => Socket.Connected;
    }

    public void Bind(EndPoint localEP)
    {
        Socket.Bind(localEP);
    }

    public ValueTask ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken)
    {
        return Socket.ConnectAsync(remoteEP, cancellationToken);
    }

    public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
    {
        Socket.SetSocketOption(optionLevel, optionName, optionValue);
    }

    public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
    {
        Socket.SetSocketOption(optionLevel, optionName, optionValue);
    }

    public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default)
    {
        return Socket.SendAsync(buffer, socketFlags, cancellationToken);
    }

    public ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default)
    {
        return Socket.ReceiveAsync(buffer, socketFlags, cancellationToken);
    }

    public void Dispose()
    {
        Socket.Dispose();
    }
}