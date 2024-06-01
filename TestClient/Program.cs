using BigMission.RedMist.DriveSync.Multicast;
using BigMission.RedMist.DriveSync.Multicast.Client;
using BigMission.RedMist.DriveSync.Multicast.DriveSync;
using BigMission.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestClient;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Client starting...");
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<ISocketFactory, SocketFactory>();
        builder.Services.AddSingleton<MulticastComm>();
        builder.Services.AddSingleton<IDateTimeHelper, DateTimeHelper>();
        builder.Services.AddSingleton<IServerConfigurationProvider, ServerConfigurationProvider>();

        IHost host = builder.Build();
        var lf = host.Services.GetRequiredService<ILoggerFactory>();
        var comm = host.Services.GetRequiredService<MulticastComm>();
        var client = new DriveSyncClient(comm, lf, new DateTimeHelper());
        client.OnReceivedChannelStatus += (channels) =>
        {
            Console.WriteLine($"Received {channels.Values.Length} channels");
            return Task.CompletedTask;
        };
        client.OnReceivedChannelMappings += (mappings) =>
        {
            Console.WriteLine($"Received {mappings.Mappings.Length} mappings");
            return Task.CompletedTask;
        };
        CancellationTokenSource cts = new();
        client.StartReceive(cts.Token);

        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(3000);
            await client.RequestChannelMappings();

            await Task.Delay(2000);
            client.SubscribeToStatus();

        }
        cts.Cancel();
        host.Run();
    }
}
public class ServerConfigurationProvider : IServerConfigurationProvider
{
    public ServerConfiguration GetServerConfiguration()
    {
        return new ServerConfiguration
        {
            ApplicationInstance = new Guid("e554aa67-614a-43db-a39d-d863c7a0ffca"),
            MulticastAddress = "224.0.5.2",
            MulticastPort = 32928
        };
    }
}