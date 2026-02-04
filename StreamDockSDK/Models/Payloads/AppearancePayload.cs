using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Payload for appearance events (willAppear, willDisappear)
///     Received when an action instance appears or disappears on the device
/// </summary>
public class AppearancePayload : BasePayload
{
    /// <summary>
    ///     Controller type identifier
    ///     Possible values: "Keypad", "Information", "SecondaryScreen", "Knob"
    /// </summary>
    [JsonPropertyName("controller")]
    public string Controller { get; set; } = string.Empty;

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