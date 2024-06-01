using ProtoBuf;

namespace BigMission.RedMist.DriveSync.Multicast.Models;

[ProtoContract]
public class ChannelMapping
{
    [ProtoMember(1)]
    public int Id { get; set; }
    [ProtoMember(2)]
    public string ReservedName { get; set; } = string.Empty;
    [ProtoMember(3)]
    public string ChannelName { get; set; } = string.Empty;
    [ProtoMember(4)]
    public float LowRange { get; set; }
    [ProtoMember(5)]
    public float HighRange { get; set; }
}

[ProtoContract]
public class ChannelMappingCollection
{
    [ProtoMember(1)]
    public ChannelMapping[] Mappings { get; set; } = [];
}