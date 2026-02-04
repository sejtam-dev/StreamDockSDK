using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Payload for dial rotation events (dialRotate)
///     Received when a user rotates a dial (knob)
/// </summary>
public class DialRotatePayload : BasePayload
{
    /// <summary>
    ///     Controller type identifier (typically "Encoder" for dial/knob controls)
    /// </summary>
    [JsonPropertyName("controller")]
    public string Controller { get; set; } = "Encoder";

    /// <summary>
    ///     Number of rotation ticks
    ///     Positive value = clockwise rotation
    ///     Negative value = counterclockwise rotation
    ///     0 = no rotation
    /// </summary>
    [JsonPropertyName("ticks")]
    public int Ticks { get; set; }

    /// <summary>
    ///     Indicates whether the knob was pressed down during rotation
    /// </summary>
    [JsonPropertyName("pressed")]
    public bool Pressed { get; set; }
}