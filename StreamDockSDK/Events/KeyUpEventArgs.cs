using StreamDockSDK.Events.Base;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for keyUp event
///     Received when a user releases a physical key
/// </summary>
public class KeyUpEventArgs : ActionEventArgs
{
    /// <summary>
    ///     Event payload containing key release information
    /// </summary>
    public KeyPayload Payload { get; set; } = new();

    /// <summary>
    ///     Get coordinates of the released key
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
    ///     Get current state (for multi-state actions)
    /// </summary>
    /// <returns>State value or null if not a multi-state action</returns>
    public int? GetState()
    {
        return Payload.State;
    }

    /// <summary>
    ///     Check if this action is inside a Multi-Action
    /// </summary>
    /// <returns>True if inside a Multi-Action</returns>
    public bool IsInMultiAction()
    {
        return Payload.IsInMultiAction;
    }
}