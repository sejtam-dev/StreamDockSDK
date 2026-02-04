using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Device size information (rows and columns)
/// </summary>
public class DeviceSize
{
    /// <summary>
    ///     Number of columns on the device (width)
    /// </summary>
    [JsonPropertyName("columns")]
    public int Columns { get; set; }

    /// <summary>
    ///     Number of rows on the device (height)
    /// </summary>
    [JsonPropertyName("rows")]
    public int Rows { get; set; }
}

/// <summary>
///     Device information for deviceDidConnect event
///     Contains details about the connected StreamDock hardware
/// </summary>
public class DeviceInfo
{
    /// <summary>
    ///     Name/model of the device (e.g., "AKP05EV25")
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Type identifier of the device (numeric ID)
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    ///     Size of the device (rows and columns)
    /// </summary>
    [JsonPropertyName("size")]
    public DeviceSize? Size { get; set; }
}