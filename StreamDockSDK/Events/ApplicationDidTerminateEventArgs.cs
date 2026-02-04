using StreamDockSDK.Events.Base;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK.Events;

/// <summary>
///     Event args for applicationDidTerminate event
///     Received when a monitored application (defined in manifest.json) terminates
/// </summary>
public class ApplicationDidTerminateEventArgs : BaseStreamDockEventArgs
{
    /// <summary>
    ///     Event payload containing application information
    /// </summary>
    public ApplicationPayload Payload { get; set; } = new();

    /// <summary>
    ///     Get the application identifier
    /// </summary>
    /// <returns>Application identifier (Bundle ID on macOS, EXE name on Windows)</returns>
    public string GetApplication()
    {
        return Payload.Application;
    }
}