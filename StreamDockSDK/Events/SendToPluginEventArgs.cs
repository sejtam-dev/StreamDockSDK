using System.Text.Json;
using StreamDockSDK.Events.Base;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for sendToPlugin event
///     Received by the plugin when the Property Inspector sends custom data (via sendToPlugin in JavaScript)
/// </summary>
public class SendToPluginEventArgs : ActionEventArgs
{
    /// <summary>
    ///     Custom JSON data sent from the Property Inspector
    ///     Access via Payload.GetProperty() or deserialize to your own type
    /// </summary>
    public JsonElement Payload { get; set; }

    /// <summary>
    ///     Try to get a property value from the payload
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>JsonElement if found, null otherwise</returns>
    public JsonElement? TryGetProperty(string propertyName)
    {
        if (Payload.ValueKind == JsonValueKind.Object &&
            Payload.TryGetProperty(propertyName, out var value))
            return value;
        return null;
    }

    /// <summary>
    ///     Deserialize payload to a specific type
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <returns>Deserialized object or default</returns>
    public T? DeserializePayload<T>() where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(Payload.GetRawText());
        }
        catch
        {
            return default;
        }
    }
}