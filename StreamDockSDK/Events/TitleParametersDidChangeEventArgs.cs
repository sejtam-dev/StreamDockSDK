using StreamDockSDK.Events.Base;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for titleParametersDidChange event
///     Received when the user changes the title or title styling of an action instance
/// </summary>
public class TitleParametersDidChangeEventArgs : ActionEventArgs
{
    /// <summary>
    ///     Event payload containing title and styling information
    /// </summary>
    public TitleParametersPayload Payload { get; set; } = new();

    /// <summary>
    ///     Get coordinates of the action
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
    ///     Get the new title text
    /// </summary>
    /// <returns>Title string</returns>
    public string GetTitle()
    {
        return Payload.Title;
    }

    /// <summary>
    ///     Get title styling parameters
    /// </summary>
    /// <returns>TitleParameters or null if not available</returns>
    public TitleParameters? GetTitleParameters()
    {
        return Payload.TitleParameters;
    }
}