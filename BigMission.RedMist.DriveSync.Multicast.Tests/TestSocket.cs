using System.Net;
using System.Net.Sockets;

namespace BigMission.RedMist.DriveSync.Multicast.Tests;

internal class TestSocket : ISocket
{
    public bool Connected { get; set; }

    public void Bind(EndPoint localEP) { }

    public ValueTask ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    public void Dispose() { }

    public byte[] Bytes { get; set; } = [];
    public int Count { get; set; }

    public ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default)
    {
        Bytes.CopyTo(buffer.Span);
        return new ValueTask<int>(Count);
    }

    public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default)
    {
        return new ValueTask<int>(Count);
    }

    public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue) { }

    public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue) { }
}
