using StreamDockSDK.Events.Base;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for deviceDidConnect event
///     Received when a device is plugged into the computer
/// </summary>
public class DeviceDidConnectEventArgs : BaseStreamDockEventArgs
{
    /// <summary>
    ///     Unique device identifier
    /// </summary>
    public string Device { get; set; } = string.Empty;

    /// <summary>
    ///     Information about the connected device
    /// </summary>
    public DeviceInfo? DeviceInfo { get; set; }

    /// <summary>
    ///     Get the device name/model
    /// </summary>
    /// <returns>Device name or empty string if not available</returns>
    public string GetDeviceName()
    {
        return DeviceInfo?.Name ?? string.Empty;
    }

    /// <summary>
    ///     Get device size (rows and columns)
    /// </summary>
    /// <returns>DeviceSize or null if not available</returns>
    public DeviceSize? GetDeviceSize()
    {
        return DeviceInfo?.Size;
    }

    /// <summary>
    ///     Get device type identifier
    /// </summary>
    /// <returns>Device type ID or 0 if not available</returns>
    public int GetDeviceType()
    {
        return DeviceInfo?.Type ?? 0;
    }
}