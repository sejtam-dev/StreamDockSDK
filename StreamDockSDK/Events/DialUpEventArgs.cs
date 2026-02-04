using StreamDockSDK.Events.Base;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for dialUp event
///     Received when a user releases a pressed dial (knob)
/// </summary>
public class DialUpEventArgs : ActionEventArgs
{
    /// <summary>
    ///     Event payload containing dial release information
    /// </summary>
    public DialPressPayload Payload { get; set; } = new();

    /// <summary>
    ///     Get coordinates of the dial
    /// </summary>
    /// <returns>Coordinates or null if not available</returns>
    public Coordinates? GetCoordinates()
    {
        return Payload.Coordinates;
    }

    /// <summary>
    ///     Get settings for this action instance
    /// </summary>
    /// <returns>Settings dictionary or null if not available</returns>
    public Dictionary<string, object>? GetSettings()
    {
        return Payload.Settings;
    }

    /// <summary>
    ///     Get controller type (typically "Encoder")
    /// </summary>
    /// <returns>Controller type string</returns>
    public string GetControllerType()
    {
        return Payload.Controller;
    }
}