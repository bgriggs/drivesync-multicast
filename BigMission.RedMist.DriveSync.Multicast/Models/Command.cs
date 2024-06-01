using ProtoBuf;

namespace BigMission.RedMist.DriveSync.Multicast.Models;

[ProtoContract]
public class Command()
{
    public static readonly string SendChannelMappings = "chmaps";
    public static readonly string ChannelStatusType = "statussub";

    [ProtoMember(1)]
    public string Type { get; set; } = string.Empty;
    [ProtoMember(2)]
    public Guid CommandDestination { get; set; } = Guid.Empty;
}
