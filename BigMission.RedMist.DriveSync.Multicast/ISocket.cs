using System.Net;
using System.Net.Sockets;

namespace BigMission.RedMist.DriveSync.Multicast;

public interface ISocket
{
    bool Connected { get; }

    void Bind(EndPoint localEP);
    ValueTask ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken);
    void Dispose();
    ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default);
    ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default);
    void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue);
    void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue);
}