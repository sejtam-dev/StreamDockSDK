using System.Text.Json.Serialization;

namespace StreamDockSDK.Models;

/// <summary>
///     Plugin metadata information
///     Contains version, available actions, and OS information
/// </summary>
public class PluginInfo
{
    /// <summary>
    ///     Plugin version string (e.g., "1.0.0")
    /// </summary>
    [JsonPropertyName("Version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    ///     List of actions provided by this plugin
    /// </summary>
    [JsonPropertyName("Actions")]
    public List<ActionInfo> Actions { get; set; } = new();

    /// <summary>
    ///     Operating system information
    /// </summary>
    [JsonPropertyName("OS")]
    public string OS { get; set; } = string.Empty;
}