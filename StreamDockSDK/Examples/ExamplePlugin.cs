using log4net;
using StreamDockSDK.Events;

namespace StreamDockSDK.Examples;

/// <summary>
///     Example plugin showing proper event subscription pattern
/// </summary>
public class ExamplePlugin : StreamDockPlugin
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ExamplePlugin));
    private int _rotationValue;

    public override void RegisterEventHandlers()
    {
        base.RegisterEventHandlers();

        Connection.Connected += OnConnected;
        Connection.Disconnected += OnDisconnected;

        Connection.WillAppear += OnWillAppear;

        Connection.KeyDown += OnKeyDown;
        Connection.KeyUp += OnKeyUp;

        Connection.DialRotate += OnDialRotate;
        Connection.DialDown += OnDialDown;
        Connection.DialUp += OnDialUp;

        Connection.DidReceiveSettings += OnDidReceiveSettings;
        Connection.DidReceiveGlobalSettings += OnDidReceiveGlobalSettings;

        Connection.DeviceDidConnect += OnDeviceDidConnect;
        Connection.DeviceDidDisconnect += OnDeviceDidDisconnect;

        Connection.ApplicationDidLaunch += OnApplicationDidLaunch;
        Connection.ApplicationDidTerminate += OnApplicationDidTerminate;

        Connection.SystemDidWakeUp += OnSystemDidWakeUp;
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        Log.Info("✓ Connected to StreamDock!");
    }

    private void OnDisconnected(object? sender, EventArgs e)
    {
        Log.Warn("✗ Disconnected from StreamDock");
    }

    private async Task OnWillAppear(object? sender, WillAppearEventArgs e)
    {
        Log.Info($"Action appeared: {e.Action}");
        Log.Debug($"  Position: [{e.GetCoordinates()?.Row},{e.GetCoordinates()?.Column}]");
        Log.Debug($"  Controller: {e.GetControllerType()}");
        Log.Debug($"  Multi-Action: {e.IsInMultiAction()}");

        // Initialize the action
        await Connection.SetTitleAsync(e.Context, "Ready!");
        await Connection.ShowOkAsync(e.Context);
    }

    private async Task OnKeyDown(object? sender, KeyDownEventArgs e)
    {
        var coords = e.GetCoordinates();
        Log.Info($"Key DOWN: {e.Action} at [{coords?.Row},{coords?.Column}]");

        // Toggle state if it's a multi-state action
        var state = e.GetState();
        if (state.HasValue)
        {
            var newState = state.Value == 0 ? 1 : 0;
            Log.Debug($"Toggling state from {state.Value} to {newState}");
            await Connection.SetStateAsync(e.Context, newState);
            await Connection.SetTitleAsync(e.Context, $"State: {newState}");
        }
    }

    private async Task OnKeyUp(object? sender, KeyUpEventArgs e)
    {
        Log.Info($"Key UP: {e.Action}");
        await Connection.ShowOkAsync(e.Context);
    }

    private async Task OnDialRotate(object? sender, DialRotateEventArgs e)
    {
        _rotationValue += e.GetTicks();

        if (e.IsClockwise())
            Log.Info($"Dial rotated CLOCKWISE: {e.GetTicks()} ticks (total: {_rotationValue})");
        else if (e.IsCounterclockwise())
            Log.Info($"Dial rotated COUNTERCLOCKWISE: {e.GetTicks()} ticks (total: {_rotationValue})");

        if (e.IsPressed()) Log.Debug("  (pressed while rotating)");

        // Update display
        await Connection.SetTitleAsync(e.Context, $"Value: {_rotationValue}");

        // Update feedback for encoder display
        await Connection.SetFeedbackAsync(e.Context, new Dictionary<string, object>
        {
            { "title", "Volume" },
            { "value", $"{_rotationValue}%" }
        });
    }

    private async Task OnDialDown(object? sender, DialDownEventArgs e)
    {
        Log.Info($"Dial pressed! Controller: {e.GetControllerType()}");

        // Reset value on press
        _rotationValue = 0;
        await Connection.SetTitleAsync(e.Context, "Reset!");
        await Connection.ShowOkAsync(e.Context);
    }

    private Task OnDialUp(object? sender, DialUpEventArgs e)
    {
        Log.Info("Dial released");
        return Task.CompletedTask;
    }

    private Task OnDidReceiveSettings(object? sender, DidReceiveSettingsEventArgs e)
    {
        Log.Info("Received settings:");
        var settings = e.GetSettings();
        if (settings != null)
            foreach (var setting in settings)
                Log.Debug($"  {setting.Key} = {setting.Value}");
        return Task.CompletedTask;
    }

    private Task OnDidReceiveGlobalSettings(object? sender, DidReceiveGlobalSettingsEventArgs e)
    {
        Log.Info("Received global settings:");
        var settings = e.GetSettings();
        if (settings != null)
            foreach (var setting in settings)
                Log.Debug($"  {setting.Key} = {setting.Value}");
        return Task.CompletedTask;
    }

    private Task OnDeviceDidConnect(object? sender, DeviceDidConnectEventArgs e)
    {
        Log.Info($"Device connected: {e.Device}");
        Log.Info($"  Name: {e.GetDeviceName()}");

        var size = e.GetDeviceSize();
        if (size != null) Log.Info($"  Size: {size.Columns}x{size.Rows}");

        Log.Info($"  Type: {e.GetDeviceType()}");
        return Task.CompletedTask;
    }

    private Task OnDeviceDidDisconnect(object? sender, DeviceDidDisconnectEventArgs e)
    {
        Log.Warn($"Device disconnected: {e.Device}");
        return Task.CompletedTask;
    }

    private Task OnSystemDidWakeUp(object? sender, EventArgs e)
    {
        Log.Info("System woke up from sleep");
        return Task.CompletedTask;
    }

    private Task OnApplicationDidLaunch(object? sender, ApplicationDidLaunchEventArgs e)
    {
        Log.Info($"Application launched: {e.GetApplication()}");
        return Task.CompletedTask;
    }

    private Task OnApplicationDidTerminate(object? sender, ApplicationDidTerminateEventArgs e)
    {
        Log.Info($"Application terminated: {e.GetApplication()}");
        return Task.CompletedTask;
    }
}

// Usage:
// class Program
// {
//     static async Task Main(string[] args)
//     {
//         var plugin = new ExamplePlugin();
//         await plugin.RunAsync(args);
//     }
// }