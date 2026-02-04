using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Title styling parameters for an action
/// </summary>
public class TitleParameters
{
    /// <summary>
    ///     Font family name (e.g., "Arial", "HarmonyOS Sans")
    /// </summary>
    [JsonPropertyName("fontFamily")]
    public string FontFamily { get; set; } = string.Empty;

    /// <summary>
    ///     Font size in points
    /// </summary>
    [JsonPropertyName("fontSize")]
    public int FontSize { get; set; }

    /// <summary>
    ///     Font style (e.g., "Regular", "Bold", "Italic")
    /// </summary>
    [JsonPropertyName("fontStyle")]
    public string FontStyle { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the title text should be underlined
    /// </summary>
    [JsonPropertyName("fontUnderline")]
    public bool FontUnderline { get; set; }

    /// <summary>
    ///     Whether the title is visible
    /// </summary>
    [JsonPropertyName("showTitle")]
    public bool ShowTitle { get; set; }

    /// <summary>
    ///     Title alignment position (e.g., "top", "bottom", "middle")
    /// </summary>
    [JsonPropertyName("titleAlignment")]
    public string TitleAlignment { get; set; } = string.Empty;

    /// <summary>
    ///     Title color in hex format (e.g., "#ffffff" for white)
    /// </summary>
    [JsonPropertyName("titleColor")]
    public string TitleColor { get; set; } = string.Empty;
}

/// <summary>
///     Payload for titleParametersDidChange event
///     Received when the user changes the title or title styling of an action instance
/// </summary>
public class TitleParametersPayload : BasePayload
{
    /// <summary>
    ///     Current state of the action (for multi-state actions, starting from 0)
    ///     Only set if the action has multiple states defined in manifest.json
    /// </summary>
    [JsonPropertyName("state")]
    public int? State { get; set; }

    /// <summary>
    ///     The new title text for the action
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Object containing title styling parameters
    /// </summary>
    [JsonPropertyName("titleParameters")]
    public TitleParameters? TitleParameters { get; set; }
}