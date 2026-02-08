using System.Text.Json.Serialization;

namespace StreamDockSDK.Attributes;

/// <summary>
///     Defines a StreamDock action. Apply this attribute to action handler classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SDActionAttribute : Attribute
{
    /// <summary>
    ///     Unique action identifier. If a package ID is defined in the plugin attribute,
    ///     it will be automatically prepended (e.g., "action1" becomes "com.example.plugin.action1").
    /// </summary>
    [JsonPropertyName("UUID")]
    public required string Uuid { get; set; }

    /// <summary>
    ///     Relative path to the action's icon (40px x 40px recommended).
    /// </summary>
    [JsonPropertyName("Icon")]
    public required string Icon { get; set; }

    /// <summary>
    ///     Display name of the action visible to users.
    /// </summary>
    [JsonPropertyName("Name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Default state index (corresponds to the index in States array).
    /// </summary>
    [JsonPropertyName("State")]
    public int? State { get; set; }

    /// <summary>
    ///     Relative path to the Property Inspector HTML file for this action.
    /// </summary>
    [JsonPropertyName("PropertyInspectorPath")]
    public string? PropertyInspectorPath { get; set; }

    /// <summary>
    ///     Whether this action can be used inside multi-actions/operation flows.
    /// </summary>
    [JsonPropertyName("SupportedInMultiActions")]
    public bool SupportedInMultiActions { get; set; } = false;

    /// <summary>
    ///     Tooltip text shown when hovering over the action in the action list.
    /// </summary>
    [JsonPropertyName("Tooltip")]
    public string? Tooltip { get; set; }

    /// <summary>
    ///     Default settings for the action (used for initialization and persistence).
    /// </summary>
    [JsonPropertyName("Settings")]
    public Dictionary<string, object>? Settings { get; set; }

    /// <summary>
    ///     Whether the user can customize the title in the Property Inspector.
    /// </summary>
    [JsonPropertyName("UserTitleEnabled")]
    public bool UserTitleEnabled { get; set; } = true;

    /// <summary>
    ///     Array of supported controller types: "Keypad", "Information", "SecondaryScreen", "Knob", "btn".
    ///     Default is ["Keypad"].
    /// </summary>
    [JsonPropertyName("Controllers")]
    public string[]? Controllers { get; set; }

    /// <summary>
    ///     Whether this action appears in the actions list.
    /// </summary>
    [JsonPropertyName("VisibleActionsList")]
    public bool VisibleActionsList { get; set; } = true;

    /// <summary>
    ///     Array of supported OS platforms: "mac", "windows".
    /// </summary>
    [JsonPropertyName("OS")]
    public string[]? Os { get; set; }
}