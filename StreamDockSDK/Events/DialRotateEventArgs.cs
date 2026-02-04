using StreamDockSDK.Events.Base;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for dialRotate event
///     Received when a user rotates a dial (knob)
/// </summary>
public class DialRotateEventArgs : ActionEventArgs
{
    /// <summary>
    ///     Event payload containing dial rotation information
    /// </summary>
    public DialRotatePayload Payload { get; set; } = new();

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
    ///     Get rotation ticks (positive = clockwise, negative = counterclockwise)
    /// </summary>
    /// <returns>Number of ticks</returns>
    public int GetTicks()
    {
        return Payload.Ticks;
    }

    /// <summary>
    ///     Check if rotation is clockwise
    /// </summary>
    /// <returns>True if clockwise</returns>
    public bool IsClockwise()
    {
        return Payload.Ticks > 0;
    }

    /// <summary>
    ///     Check if rotation is counterclockwise
    /// </summary>
    /// <returns>True if counterclockwise</returns>
    public bool IsCounterclockwise()
    {
        return Payload.Ticks < 0;
    }

    /// <summary>
    ///     Check if dial was pressed during rotation
    /// </summary>
    /// <returns>True if pressed</returns>
    public bool IsPressed()
    {
        return Payload.Pressed;
    }
}