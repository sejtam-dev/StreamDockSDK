using System.Text.Json.Serialization;

namespace StreamDockSDK.Attributes;

/// <summary>
/// Defines a StreamDock plugin. Apply this attribute to your main plugin class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SDPluginAttribute : Attribute
{
    /// <summary>
    /// Package ID used as prefix for action UUIDs (e.g., "com.example.plugin").
    /// If set, will be automatically prepended to all action UUIDs.
    /// </summary>
    [JsonIgnore]
    public required string PackageId { get; set; }

    /// <summary>
    /// Plugin author name.
    /// </summary>
    [JsonPropertyName("Author")] public required string Author { get; set; }

    /// <summary>
    /// Relative path to the plugin executable. If not specified, auto-detected from assembly name.
    /// </summary>
    [JsonPropertyName("CodePath")] public string? CodePath { get; set; }

    /// <summary>
    /// General description of the plugin's functionality.
    /// </summary>
    [JsonPropertyName("Description")] public required string Description { get; set; }

    /// <summary>
    /// Relative path to the plugin icon (128px x 128px recommended).
    /// </summary>
    [JsonPropertyName("Icon")] public required string Icon { get; set; }

    /// <summary>
    /// Display name of the plugin.
    /// </summary>
    [JsonPropertyName("Name")] public required string Name { get; set; }

    /// <summary>
    /// Plugin version (e.g., "1.0.0").
    /// </summary>
    [JsonPropertyName("Version")] public required string Version { get; set; }

    /// <summary>
    /// SDK version (currently 1).
    /// </summary>
    [JsonPropertyName("SDKVersion")] public required int SdkVersion { get; set; } = 1;

    /// <summary>
    /// Minimum required version of the Stream Dock application (e.g., "2.10.179.426").
    /// </summary>
    [JsonPropertyName("Software")] public string? MinimumVersionOfSoftware { get; set; }

    /// <summary>
    /// Custom category name under which actions will appear. If not set, defaults to "Custom".
    /// </summary>
    [JsonPropertyName("Category")] public string? Category { get; set; }

    /// <summary>
    /// Relative path to the category icon (48px x 48px recommended).
    /// </summary>
    [JsonPropertyName("CategoryIcon")] public string? CategoryIcon { get; set; }

    /// <summary>
    /// CodePath specifically for macOS.
    /// </summary>
    [JsonPropertyName("CodePathMac")] public string? CodePathMac { get; set; }

    /// <summary>
    /// CodePath specifically for Windows.
    /// </summary>
    [JsonPropertyName("CodePathWin")] public string? CodePathWin { get; set; }

    /// <summary>
    /// Relative path to the global Property Inspector HTML file.
    /// </summary>
    [JsonPropertyName("PropertyInspectorPath")] public string? PropertyInspectorPath { get; set; }

    /// <summary>
    /// Website URL providing more information about the plugin.
    /// </summary>
    [JsonPropertyName("URL")] public string? Url { get; set; }

    /// <summary>
    /// Dictionary of applications to monitor (not directly used; use SDPluginApplicationsToMonitorAttribute instead).
    /// </summary>
    [JsonPropertyName("ApplicationsToMonitor")] public Dictionary<string, object>? ApplicationsToMonitor { get; set; }
}