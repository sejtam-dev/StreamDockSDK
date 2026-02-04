using System.Text.Json.Serialization;

namespace StreamDockSDK.Models;

/// <summary>
///     Base class for all StreamDock events sent via WebSocket
/// </summary>
public class StreamDockEvent
{
    [JsonPropertyName("event")] public string Event { get; set; } = string.Empty;

    [JsonPropertyName("context")] public string Context { get; set; } = string.Empty;

    [JsonPropertyName("action")] public string? Action { get; set; }

    [JsonPropertyName("device")] public string? Device { get; set; }

    [JsonPropertyName("payload")] public object? Payload { get; set; }

    [JsonPropertyName("deviceInfo")] public object? DeviceInfo { get; set; }
}