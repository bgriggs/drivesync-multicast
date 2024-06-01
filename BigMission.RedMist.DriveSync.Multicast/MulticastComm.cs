using BigMission.RedMist.DriveSync.Multicast.DriveSync;
using BigMission.RedMist.DriveSync.Multicast.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace BigMission.RedMist.DriveSync.Multicast;

public class MulticastComm : IDisposable, IMulticastComm
{
    private const int MTU = 4096;
    private const byte VERSION = 1;
    private readonly ISocket rxSocket;
    private readonly ISocket txSocket;
    private readonly IPEndPoint remoteGroupEndpoint;
    private readonly Guid id;
    private readonly ILogger? logger;

    public event Func<PayloadHeader, Task>? OnReceived;

    public MulticastComm(IServerConfigurationProvider serverConfigurationProvider, ISocketFactory socketFactory, ILoggerFactory? loggerFactory = null)
    {
        var serverConfiguration = serverConfigurationProvider.GetServerConfiguration();
        var address = IPAddress.Parse(serverConfiguration.MulticastAddress);
        remoteGroupEndpoint = new IPEndPoint(address, serverConfiguration.MulticastPort);

        rxSocket = socketFactory.CreateSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        rxSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address));

        txSocket = socketFactory.CreateSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        txSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address));

        id = serverConfiguration.ApplicationInstance;
        logger = loggerFactory?.CreateLogger(GetType().Name);
    }

    public async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        logger?.LogTrace("Receiving messages started...");
        rxSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        var localEp = new IPEndPoint(IPAddress.Any, remoteGroupEndpoint.Port);
        rxSocket.Bind(localEp);
        while (!cancellationToken.IsCancellationRequested)
        {
            var buffer = new byte[MTU];
            var received = await rxSocket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
            logger?.LogTrace($"Received {received} bytes");

            // We need to check if the received message is at least 18 bytes long to account for the version byte, the source Guid and data type byte
            if (received > 18)
            {
                byte version = buffer[0];

                if (version != VERSION)
                {
                    logger?.LogWarning($"Received message with unsupported version {version}");
                    continue;
                }

                Guid source = new(buffer[1..17]);
                byte typeIndex = buffer[17];

                // Skip messages from self
                if (source == id) continue;

                var data = new byte[received - 18];
                Array.Copy(buffer, 18, data, 0, received - 18);
                var payloadWithHeader = new PayloadHeader(version, source, typeIndex, data);
                var _ = Task.Run(async () =>
                {
                    if (OnReceived != null)
                    {
                        await OnReceived.Invoke(payloadWithHeader).ConfigureAwait(false);
                    }
                }, cancellationToken);
            }
        }
    }

    public async Task SendAsync(byte[] payload, byte typeIndex, CancellationToken cancellationToken = default)
    {
        if (!txSocket.Connected)
        {
            await txSocket.ConnectAsync(remoteGroupEndpoint, cancellationToken).ConfigureAwait(false);
        }

        if (payload.Length == 0) throw new ArgumentException("Message must not be empty");

        var data = new List<byte> { VERSION };
        data.AddRange(id.ToByteArray());
        data.Add(typeIndex);
        data.AddRange(payload);

        if (data.Count > MTU) throw new ArgumentException("Message is too large");

        await txSocket.SendAsync(data.ToArray(), SocketFlags.None, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        rxSocket.Dispose();
        txSocket.Dispose();
        GC.SuppressFinalize(this);
    }
}
