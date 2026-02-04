using StreamDockSDK.Events.Base;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for didReceiveGlobalSettings event
///     Received after the global plugin settings are changed
/// </summary>
public class DidReceiveGlobalSettingsEventArgs : BaseStreamDockEventArgs
{
    /// <summary>
    ///     Event payload containing global settings
    /// </summary>
    public GlobalSettingsPayload Payload { get; set; } = new();

    /// <summary>
    ///     Get global settings for the entire plugin
    /// </summary>
    /// <returns>Settings dictionary or null if not available</returns>
    public Dictionary<string, object>? GetSettings()
    {
        return Payload.Settings;
    }
}