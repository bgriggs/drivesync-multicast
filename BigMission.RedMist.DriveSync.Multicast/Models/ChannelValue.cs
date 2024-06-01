using ProtoBuf;

namespace BigMission.RedMist.DriveSync.Multicast.Models;

[ProtoContract]
public class ChannelValue
{
    [ProtoMember(1)]
    public int Id { get; set; }
    [ProtoMember(2)]
    public float Value { get; set; }
}

[ProtoContract]
public class ChannelValueCollection
{
    [ProtoMember(1)]
    public ChannelValue[] Values { get; set; } = [];
}