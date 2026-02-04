namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Generic payload for sendToPlugin event (contains custom data from Property Inspector)
/// </summary>
public class SendToPluginPayload
{
    // This is a dynamic payload - can contain any custom data
    // Will be deserialized as Dictionary<string, object> or JsonElement
}