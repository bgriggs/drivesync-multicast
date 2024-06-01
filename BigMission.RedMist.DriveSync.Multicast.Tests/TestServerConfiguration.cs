using BigMission.RedMist.DriveSync.Multicast.DriveSync;

namespace BigMission.RedMist.DriveSync.Multicast.Tests;

internal class TestServerConfiguration : IServerConfigurationProvider
{
    public ServerConfiguration GetServerConfiguration()
    {
        return new ServerConfiguration
        {
            ApplicationInstance = new Guid("cd06df75-be28-4bbe-a9fc-130287196f45"),
            MulticastAddress = "1.2.3.4",
            MulticastPort = 1234
        };
    }
}
