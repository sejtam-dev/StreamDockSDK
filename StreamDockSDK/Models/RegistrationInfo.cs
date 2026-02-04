using System.Text.Json.Serialization;

namespace StreamDockSDK.Models;

/// <summary>
///     Registration information passed when plugin starts
///     Used to register the plugin with StreamDock via WebSocket
/// </summary>
public class RegistrationInfo
{
    /// <summary>
    ///     Event type for registration (typically "registerPlugin")
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = "registerPlugin";

    /// <summary>
    ///     Unique identifier for this plugin instance (UUID)
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;
}