using BigMission.RedMist.DriveSync.Multicast.Models;

namespace BigMission.RedMist.DriveSync.Multicast.DriveSync;

public interface IChannelProvider
{
    ChannelMappingCollection GetChannelMappings();
    ChannelValueCollection GetChannelValues();
}
