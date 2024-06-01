namespace BigMission.RedMist.DriveSync.Multicast.DriveSync;

public class ServerConfiguration
{
    public Guid ApplicationInstance{get;set; } = Guid.Empty;
    public string MulticastAddress { get; set; } = "";
    public int MulticastPort { get; set; } = 0;
}
