using StreamDockSDK.Events.Base;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for deviceDidDisconnect event
///     Received when a device is unplugged from the computer
/// </summary>
public class DeviceDidDisconnectEventArgs : BaseStreamDockEventArgs
{
    /// <summary>
    ///     Unique identifier of the disconnected device
    /// </summary>
    public string Device { get; set; } = string.Empty;
}