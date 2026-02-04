using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Payload for dial/encoder press events (dialDown, dialUp)
///     Received when a user presses or releases a dial (knob)
/// </summary>
public class DialPressPayload : BasePayload
{
    /// <summary>
    ///     Controller type identifier (typically "Encoder" for dial/knob controls)
    /// </summary>
    [JsonPropertyName("controller")]
    public string Controller { get; set; } = "Encoder";
}