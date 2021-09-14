namespace Artemis.Plugins.Modules.Fallout4.Enums
{
    public enum FalloutPacketType : byte
    {
        Heartbeat = 0,
        NewConnection = 1,
        Busy = 2,
        DataUpdate = 3,
        MapUpdate = 4,
        CommandRequest = 5,
        CommandResponse = 6
    }
}
