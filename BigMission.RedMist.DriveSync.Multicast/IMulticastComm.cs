using BigMission.RedMist.DriveSync.Multicast.Models;

namespace BigMission.RedMist.DriveSync.Multicast
{
    public interface IMulticastComm
    {
        event Func<PayloadHeader, Task>? OnReceived;

        Task ReceiveMessagesAsync(CancellationToken cancellationToken);
        Task SendAsync(byte[] payload, byte typeIndex, CancellationToken cancellationToken = default);
    }
}