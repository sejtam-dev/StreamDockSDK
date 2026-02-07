using System.Text.Json.Serialization;

namespace StreamDockSDK.Attributes;

/// <summary>
/// Defines a state for a StreamDock action. Actions can have one or more states.
/// Multiple states can be defined by applying this attribute multiple times to the same action class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)] 
public class SDActionStateAttribute : Attribute
{
    /// <summary>
    /// Relative path to the state's image.
    /// </summary>
    [JsonPropertyName("Image")] public required string Image { get; set; }

    /// <summary>
    /// Default title text for this state.
    /// </summary>
    [JsonPropertyName("Title")] public string? Title { get; set; }

    /// <summary>
    /// Whether the title is visible for this state.
    /// </summary>
    [JsonPropertyName("ShowTitle")] public bool ShowTitle { get; set; } = true;

    /// <summary>
    /// Default title color in hex format (e.g., "#ffffff").
    /// </summary>
    [JsonPropertyName("TitleColor")] public string? TitleColor { get; set; }

    /// <summary>
    /// Title alignment: "top", "bottom", "center", "middle".
    /// </summary>
    [JsonPropertyName("TitleAlignment")] public string? TitleAlignment { get; set; }

    /// <summary>
    /// Font family name for the title.
    /// </summary>
    [JsonPropertyName("FontFamily")] public string? FontFamily { get; set; }

    /// <summary>
    /// Font style: "Regular", "Bold", "Italic", "Bold Italic".
    /// </summary>
    [JsonPropertyName("FontStyle")] public string? FontStyle { get; set; }

    /// <summary>
    /// Font size for the title.
    /// </summary>
    [JsonPropertyName("FontSize")] public int? FontSize { get; set; }

    /// <summary>
    /// Whether the title should be underlined.
    /// </summary>
    [JsonPropertyName("FontUnderline")] public bool FontUnderline { get; set; } = false;
}