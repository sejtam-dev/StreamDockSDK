using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Base payload with common properties for most events
/// </summary>
public abstract class BasePayload
{
    /// <summary>
    ///     JSON object containing persistent data stored for the action instance
    /// </summary>
    [JsonPropertyName("settings")]
    public Dictionary<string, object>? Settings { get; set; }

    /// <summary>
    ///     Coordinates of the action on the StreamDock device
    /// </summary>
    [JsonPropertyName("coordinates")]
    public Coordinates? Coordinates { get; set; }
}

/// <summary>
///     Coordinates on the StreamDock device (row and column position)
/// </summary>
public class Coordinates
{
    /// <summary>
    ///     Column position (0-based index from left to right)
    /// </summary>
    [JsonPropertyName("column")]
    public int Column { get; set; }

    /// <summary>
    ///     Row position (0-based index from top to bottom)
    /// </summary>
    [JsonPropertyName("row")]
    public int Row { get; set; }
}