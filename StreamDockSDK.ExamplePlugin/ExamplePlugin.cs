using System.Reflection;
using log4net;
using StreamDockSDK;
using StreamDockSDK.Attributes;

namespace ExamplePlugin;

/// <summary>
///     Example StreamDock Plugin demonstrating StreamDockSDK features
///     Button Actions:
///     - Counter Action: Increment counter with configurable settings
///     - Toggle Action: ON/OFF toggle with dynamic icons
///     Encoder (Knob) Actions:
///     - Volume Knob: Volume control with mute and reset
///     • Rotate: Adjust volume (0-100%)
///     • Short press: Toggle mute
///     • Long press (>500ms): Reset to default volume
///     - Timer Knob: Countdown timer (1-60 minutes)
///     • Rotate: Set duration (only when stopped)
///     • Short press: Start/Stop timer
///     • Long press (>500ms): Reset timer
///     Features Demonstrated:
///     - Automatic action handler discovery via [Action] attribute
///     - Property Inspector integration with JavaScript
///     - Long press detection using Timer pattern (hardware limitation workaround)
///     - Debounced dial rotation for smooth updates
///     - Thread-safe Timer disposal
///     - State persistence across restarts
/// </summary>
[SDPlugin(
    PackageId = "com.example.streamdockplugin",
    SdkVersion = 1,
    Name = "Example Plugin",
    Version = "1.0.0",
    Author = "Sejtam_",
    Description = "Example StreamDock plugin demonstrating StreamDockSDK features",
    Category = "Examples",
    CategoryIcon = "Assets/Icons/plugin.png",
    Icon = "Assets/Icons/plugin.png",
    CodePath = "ExamplePlugin.exe",
    MinimumVersionOfSoftware = "1.0.0"
)]
[SDPluginOS(Platform = "windows", MinimumVersion = "10")]
public class ExamplePlugin : StreamDockPlugin
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ExamplePlugin));

    /// <summary>
    ///     Register event handlers for plugin lifecycle events
    /// </summary>
    public override void RegisterEventHandlers()
    {
        base.RegisterEventHandlers();

        Connection.Connected += OnConnected;
        Connection.Disconnected += OnDisconnected;
    }

    /// <summary>
    ///     Called when plugin connects to StreamDock
    ///     Automatically discovers and registers all action handlers
    /// </summary>
    private void OnConnected(object? sender, EventArgs e)
    {
        // Automatically discover and register action handlers
        HandlerManager.DiscoverHandlers(Assembly.GetExecutingAssembly());

        Log.Info("Example Plugin connected to StreamDock");
    }

    /// <summary>
    ///     Called when plugin disconnects from StreamDock
    /// </summary>
    private void OnDisconnected(object? sender, EventArgs e)
    {
        Log.Info("Example Plugin disconnected from StreamDock");
    }
}