using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Payload for key press events (keyDown, keyUp)
///     Received when a user presses or releases a physical key
/// </summary>
public class KeyPayload : BasePayload
{
    /// <summary>
    ///     Current state of the action (for multi-state actions, starting from 0)
    ///     Only set if the action has multiple states defined in manifest.json
    /// </summary>
    [JsonPropertyName("state")]
    public int? State { get; set; }

    /// <summary>
    ///     Indicates whether this action is inside a Multi-Action
    /// </summary>
    [JsonPropertyName("isInMultiAction")]
    public bool IsInMultiAction { get; set; }
}