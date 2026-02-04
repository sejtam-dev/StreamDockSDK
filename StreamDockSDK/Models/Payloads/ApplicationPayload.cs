using System.Text.Json.Serialization;

namespace StreamDockSDK.Models.Payloads;

/// <summary>
///     Payload for applicationDidLaunch and applicationDidTerminate events
///     Contains information about monitored applications defined in manifest.json
/// </summary>
public class ApplicationPayload
{
    /// <summary>
    ///     Application identifier
    ///     - macOS: Bundle ID (e.g., "com.apple.mail")
    ///     - Windows: EXE name (e.g., "notepad.exe")
    /// </summary>
    [JsonPropertyName("application")]
    public string Application { get; set; } = string.Empty;
}