using StreamDockSDK.Events.Base;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for willDisappear event
///     Received when an action instance stops being displayed
///     (profile switch, folder exit)
/// </summary>
public class WillDisappearEventArgs : ActionEventArgs
{
    /// <summary>
    ///     Event payload containing disappearance information
    /// </summary>
    public AppearancePayload Payload { get; set; } = new();

    /// <summary>
    ///     Get coordinates where the action was displayed
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
    ///     Get controller type (Keypad, Information, SecondaryScreen, Knob)
    /// </summary>
    /// <returns>Controller type string</returns>
    public string GetControllerType()
    {
        return Payload.Controller;
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