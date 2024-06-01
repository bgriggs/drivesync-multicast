namespace BigMission.RedMist.DriveSync.Multicast.Models;

internal static class TypeMapping
{
    private static readonly Dictionary<byte, Type> typeLookup = new()
    {
        { 1, typeof(ApplicationInfo) },
        { 2, typeof(Command) },
        { 3, typeof(ChannelValueCollection) },
        { 4, typeof(ChannelMappingCollection) },
    };

    private static Dictionary<Type, byte>? typeIndexLookup;

    public static byte GetTypeIndex(Type type)
    {
        typeIndexLookup ??= typeLookup.ToDictionary(x => x.Value, x => x.Key);

        return typeIndexLookup[type];
    }

    public static Type GetType(byte index)
    {
        return typeLookup[index];
    }

    public static Type ToType(this byte typeIndex)
    {
        return GetType(typeIndex);
    }

    public static byte ToTypeIndex(this Type type)
    {
        return GetTypeIndex(type);
    }
}
