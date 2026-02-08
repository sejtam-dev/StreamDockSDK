using System.Text.Json.Serialization;

namespace StreamDockSDK.Attributes;

/// <summary>
///     Defines an operating system requirement for the plugin.
///     Multiple OS requirements can be specified by applying this attribute multiple times.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SDPluginOSAttribute : Attribute
{
    /// <summary>
    ///     Platform name: "mac" or "windows".
    /// </summary>
    [JsonPropertyName("Platform")]
    public required string Platform { get; set; }

    /// <summary>
    ///     Minimum OS version (e.g., "7" for Windows, "10.11" for macOS).
    /// </summary>
    [JsonPropertyName("MinimumVersion")]
    public required string MinimumVersion { get; set; }
}