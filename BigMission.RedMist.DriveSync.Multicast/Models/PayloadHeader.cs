namespace BigMission.RedMist.DriveSync.Multicast.Models;

public class PayloadHeader(byte version, Guid source, byte payloadType, byte[] payload)
{
    public byte Version { get; } = version;
    public Guid Source { get; } = source;
    public byte PayloadTypeIndex { get; } = payloadType;
    public byte[] Payload { get; } = payload;
}
