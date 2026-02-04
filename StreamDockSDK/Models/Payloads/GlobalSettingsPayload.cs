using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Payload for didReceiveGlobalSettings event
///     Received after the global plugin settings are changed
/// </summary>
public class GlobalSettingsPayload
{
    /// <summary>
    ///     JSON object containing global persistent data for the entire plugin
    /// </summary>
    [JsonPropertyName("settings")]
    public Dictionary<string, object>? Settings { get; set; }
}