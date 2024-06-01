using ProtoBuf;

namespace BigMission.RedMist.DriveSync.Multicast.Models;

[ProtoContract]
public class ApplicationInfo
{
    public static readonly string DriverSyncType = "DriveSync";
    public static readonly string RedmistConfiguration = "RedmistConfiguration";

    public ApplicationInfo() { }

    public ApplicationInfo(string driverSyncType, string v, Guid configurationId)
    {
        Type = driverSyncType;
        Version = v;
        CurrentConfiguration = configurationId;
    }

    [ProtoMember(1)]
    public string Type { get; set; } = string.Empty;
    [ProtoMember(2)]
    public string Version { get; set; } = string.Empty;
    [ProtoMember(3)]
    public Guid CurrentConfiguration { get; set;  } = Guid.Empty;
}
