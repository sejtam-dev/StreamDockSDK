namespace StreamDockSDK.Events;

/// <summary>
///     Event names received from StreamDock
/// </summary>
public static class StreamDockEvents
{
    // Connection events
    public const string DidReceiveSettings = "didReceiveSettings";
    public const string DidReceiveGlobalSettings = "didReceiveGlobalSettings";
    public const string DeviceDidConnect = "deviceDidConnect";
    public const string DeviceDidDisconnect = "deviceDidDisconnect";
    public const string ApplicationDidLaunch = "applicationDidLaunch";
    public const string ApplicationDidTerminate = "applicationDidTerminate";
    public const string SystemDidWakeUp = "systemDidWakeUp";

    // Action lifecycle events
    public const string WillAppear = "willAppear";
    public const string WillDisappear = "willDisappear";

    // Key/Button events
    public const string KeyDown = "keyDown";
    public const string KeyUp = "keyUp";

    // Dial/Knob events
    public const string DialRotate = "dialRotate";
    public const string DialDown = "dialDown";
    public const string DialUp = "dialUp";

    // Title parameter events
    public const string TitleParametersDidChange = "titleParametersDidChange";

    // Property inspector events
    public const string PropertyInspectorDidAppear = "propertyInspectorDidAppear";
    public const string PropertyInspectorDidDisappear = "propertyInspectorDidDisappear";
    public const string SendToPlugin = "sendToPlugin";
}