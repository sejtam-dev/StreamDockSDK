namespace StreamDockSDK.Actions;

/// <summary>
///     Attribute to mark an ActionHandler class with its corresponding action ID
///     Used for automatic registration and discovery of action handlers
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ActionAttribute : Attribute
{
    /// <summary>
    ///     Create an action attribute
    /// </summary>
    /// <param name="actionId">Action identifier matching manifest.json</param>
    public ActionAttribute(string actionId)
    {
        ActionId = actionId ?? throw new ArgumentNullException(nameof(actionId));
    }

    /// <summary>
    ///     Action identifier from manifest.json (e.g., "com.example.plugin.action1")
    /// </summary>
    public string ActionId { get; }
}