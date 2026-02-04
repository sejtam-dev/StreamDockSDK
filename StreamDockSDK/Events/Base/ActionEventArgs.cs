namespace StreamDockSDK.Events.Base;

/// <summary>
///     Base event args for action-related events (events with context, action, device)
///     Provides common properties and helper methods for action identification
/// </summary>
public abstract class ActionEventArgs : BaseStreamDockEventArgs
{
    /// <summary>
    ///     Unique identifier for this specific action instance
    /// </summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    ///     Action identifier from manifest.json (e.g., "com.example.plugin.action1")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    ///     Unique identifier for the physical device
    /// </summary>
    public string Device { get; set; } = string.Empty;

    /// <summary>
    ///     Check if this event is from a specific action type
    /// </summary>
    /// <param name="actionId">Action identifier to check</param>
    /// <returns>True if the action matches</returns>
    public bool IsAction(string actionId)
    {
        return Action == actionId;
    }
}