using System.Text.Json.Serialization;

namespace StreamDockSDK.Models;

/// <summary>
///     Action state information for multi-state actions
///     Used for toggle buttons and other actions with multiple visual states
/// </summary>
public class ActionState
{
    /// <summary>
    ///     Path to the image for this state (optional)
    /// </summary>
    [JsonPropertyName("Image")]
    public string? Image { get; set; }

    /// <summary>
    ///     Display name for this state
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}