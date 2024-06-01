using BigMission.RedMist.DriveSync.Multicast;
using BigMission.RedMist.DriveSync.Multicast.DriveSync;
using BigMission.RedMist.DriveSync.Multicast.Models;
using BigMission.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestServer;

internal class Program
{
    public static readonly Guid Id = new("01197092-a4c6-4a0a-9cc2-11c9a3150a3d");

    static void Main(string[] args)
    {
        Console.WriteLine("Server starting...");
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<IApplicationInfoProvider, ApplicationInfoProvider>();
        builder.Services.AddSingleton<MulticastComm>();
        builder.Services.AddSingleton<SubscriptionTracker>();
        builder.Services.AddSingleton<IChannelProvider, ChannelProvider>();
        builder.Services.AddSingleton<IDateTimeHelper, DateTimeHelper>();
        builder.Services.AddSingleton<IServerConfigurationProvider, ServerConfigurationProvider>();
        builder.Services.AddSingleton<ISocketFactory, SocketFactory>();
        builder.Services.AddHostedService<Listener>();

        IHost host = builder.Build();
        host.Run();
    }
}

class ApplicationInfoProvider : IApplicationInfoProvider
{
    public ApplicationInfo GetApplicationInfo()
    {
        return new ApplicationInfo(ApplicationInfo.DriverSyncType, "1.2.3", Program.Id);
    }
}

class ChannelProvider : IChannelProvider
{
    public ChannelMappingCollection GetChannelMappings()
    {
        return new ChannelMappingCollection
        {
            Mappings =
            [
                new() {
                    Id = 1,
                    ChannelName = "Channel 1",
                }
            ]
        };
    }

    public ChannelValueCollection GetChannelValues()
    {
        return new ChannelValueCollection
        {
            Values =
            [
                new ChannelValue
                {
                    Id = 1,
                    Value = 123.45f
                }
            ]
        };
    }
}



public class ServerConfigurationProvider : IServerConfigurationProvider
{
    public ServerConfiguration GetServerConfiguration()
    {
        return new ServerConfiguration
        {
            ApplicationInstance = Program.Id,
            MulticastAddress = "224.0.5.2",
            MulticastPort = 32928
        };
    }
}
