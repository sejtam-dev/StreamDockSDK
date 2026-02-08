using System.Text.Json.Serialization;

namespace StreamDockSDK.Attributes;

/// <summary>
///     Defines applications to monitor for launch/termination events.
///     Multiple entries can be specified by applying this attribute multiple times (once per OS platform).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SDPluginApplicationsToMonitorAttribute : Attribute
{
    /// <summary>
    ///     Operating system platform ("mac" or "windows")
    /// </summary>
    [JsonPropertyName("OS")]
    public required string OS { get; set; }

    /// <summary>
    ///     Comma-separated list of applications to monitor.
    ///     For macOS: bundle identifiers (e.g., "com.apple.mail")
    ///     For Windows: executable names (e.g., "notepad.exe")
    /// </summary>
    [JsonPropertyName("Applications")]
    public required string Applications { get; set; }
}