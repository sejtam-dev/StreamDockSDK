using log4net;
using StreamDockSDK;
using StreamDockSDK.Actions;

namespace ExamplePlugin.Actions;

/// <summary>
///     Toggle action that switches between ON/OFF states with dynamic icons
///     
///     Features:
///     - Toggle ON/OFF state on key press
///     - Dynamic icon changes based on state
///     - Customizable ON/OFF labels
///     - State persistence across restarts
///     
///     Settings:
///     - isOn (bool): Current ON/OFF state
///     - onLabel (string): Text to display when ON (default: "ON")
///     - offLabel (string): Text to display when OFF (default: "OFF")
/// </summary>
[Action("com.example.toggle")]
public class ToggleActionHandler : ActionHandler
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(ToggleActionHandler));
    
    /// <summary>
    ///     Current toggle state (true = ON, false = OFF)
    /// </summary>
    private bool _isOn;

    public ToggleActionHandler(StreamDockConnection connection, string context, Dictionary<string, object>? settings)
        : base(connection, context, settings)
    {
        Logger.Info($"[Toggle] Handler created - Context: {context}");
    }

    /// <summary>
    ///     Initialize toggle state when action appears on the device
    /// </summary>
    public override async Task OnWillAppearAsync()
    {
        Logger.Info($"[Toggle] Action appeared - Context: {Context}");

        // Try to load state from settings
        _isOn = GetSetting("isOn", false);

        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Called when settings are changed from Property Inspector
    /// </summary>
    public override async Task OnSettingsChangedAsync(Dictionary<string, object> settings)
    {
        Logger.Info($"[Toggle] Settings changed");
        
        // Update internal settings reference
        UpdateSettings(settings);
        
        // Reload state from settings
        _isOn = GetSetting("isOn", false);
        
        // Update display with new labels
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Toggle state when key is pressed
    /// </summary>
    public override async Task OnKeyDownAsync()
    {
        Logger.Info($"[Toggle] Key pressed - Current state: {_isOn}");
        _isOn = !_isOn;

        // Save state to settings
        var newSettings = new Dictionary<string, object>(Settings ?? new Dictionary<string, object>())
        {
            ["isOn"] = _isOn
        };

        await SetSettingsAsync(newSettings);

        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Update the display with current state (title and icon)
    /// </summary>
    public override async Task UpdateDisplayAsync()
    {
        var onLabel = GetSetting("onLabel", "ON") ?? "ON";
        var offLabel = GetSetting("offLabel", "OFF") ?? "OFF";

        var title = _isOn ? onLabel : offLabel;
        await SetTitleAsync(title);

        // Set icon based on state
        await SetStateAsync(_isOn ? 1 : 0);

        Logger.Debug($"[Toggle] Display updated - State: {title}");
    }
}