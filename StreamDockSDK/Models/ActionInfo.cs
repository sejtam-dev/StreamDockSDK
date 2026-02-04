using System.Text.Json.Serialization;

namespace StreamDockSDK.Models;

/// <summary>
///     Action metadata information
///     Describes a single action provided by the plugin
/// </summary>
public class ActionInfo
{
    /// <summary>
    ///     Unique identifier for this action (e.g., "com.example.plugin.action1")
    ///     Must match the UUID in manifest.json
    /// </summary>
    [JsonPropertyName("UUID")]
    public string UUID { get; set; } = string.Empty;

    /// <summary>
    ///     Display name of the action
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Path to the action's icon image (optional)
    /// </summary>
    [JsonPropertyName("Icon")]
    public string? Icon { get; set; }

    /// <summary>
    ///     List of states for multi-state actions (e.g., toggle buttons)
    ///     Null or empty for single-state actions
    /// </summary>
    [JsonPropertyName("States")]
    public List<ActionState>? States { get; set; }
}