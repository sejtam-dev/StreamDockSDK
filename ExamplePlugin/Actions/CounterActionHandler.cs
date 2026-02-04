using System.Text.Json;
using log4net;
using StreamDockSDK;
using StreamDockSDK.Actions;

namespace ExamplePlugin.Actions;

/// <summary>
///     Counter action that increments on key press
///     Features:
///     - Increment counter on each key press
///     - Configurable start value and increment amount
///     - Reset button in Property Inspector
///     - Display current count as title
///     Settings:
///     - startValue (int): Initial counter value (default: 0)
///     - increment (int): Amount to add on each press (default: 1)
///     - resetOnAppear (bool): Reset to start value when action appears
/// </summary>
[Action("com.example.counter")]
public class CounterActionHandler : ActionHandler
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(CounterActionHandler));

    /// <summary>
    ///     Current counter value
    /// </summary>
    private int _count;

    public CounterActionHandler(StreamDockConnection connection, string context, Dictionary<string, object>? settings)
        : base(connection, context, settings)
    {
        Logger.Info($"[Counter] Handler created - Context: {context}");
    }

    /// <summary>
    ///     Initialize counter when action appears on the device
    /// </summary>
    public override async Task OnWillAppearAsync()
    {
        Logger.Info($"[Counter] Action appeared - Context: {Context}");
        _count = GetSetting("startValue", 0);
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Called when settings are changed from Property Inspector
    /// </summary>
    public override async Task OnSettingsChangedAsync(Dictionary<string, object> settings)
    {
        Logger.Info("[Counter] Settings changed");

        // Update internal settings reference
        UpdateSettings(settings);

        // Settings changed, but we don't need to reset counter
        // Counter keeps running value, only gets reset on resetOnAppear or reset button
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Increment counter when key is pressed
    /// </summary>
    public override async Task OnKeyDownAsync()
    {
        var increment = GetSetting("increment", 1);
        Logger.Info($"[Counter] Key pressed - Count: {_count}, Increment: {increment}");
        _count += increment;
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Update the display with current counter value
    /// </summary>
    public override async Task UpdateDisplayAsync()
    {
        await SetTitleAsync(_count.ToString());
        Logger.Debug($"[Counter] Display updated - Count: {_count}");
    }

    /// <summary>
    ///     Handle custom messages from Property Inspector (e.g., reset button)
    /// </summary>
    /// <param name="payload">JSON payload from Property Inspector</param>
    public override async Task OnSendToPluginAsync(JsonElement payload)
    {
        // Check if this is a reset command
        if (payload.TryGetProperty("action", out var actionObj) && actionObj.GetString() == "resetCounter")
        {
            Logger.Info($"[Counter] Resetting counter - Context: {Context}");
            _count = GetSetting("startValue", 0);
            await UpdateDisplayAsync();
        }
    }
}