using log4net;
using StreamDockSDK;
using StreamDockSDK.Actions;
using StreamDockSDK.Attributes;

namespace ExamplePlugin.Actions;

/// <summary>
///     Volume Knob action demonstrating encoder (dial) functionality
///     Features:
///     - Rotate to adjust volume (0-100)
///     - Short press: Toggle mute
///     - Long press (>500ms): Reset to default volume
///     - Debounced rotation for smooth updates
///     - Visual feedback with circular gauge
///     Settings:
///     - volume (int): Current volume level (0-100)
///     - isMuted (bool): Mute state
///     - defaultVolume (int): Volume to reset to on long press (default: 50)
///     - longPressThreshold (int): Milliseconds to trigger long press (default: 500)
///     Note: StreamDock sends DialUp immediately after DialDown, so we use Timer
///     to measure actual press duration.
/// </summary>
[SDAction(
    Uuid = "volumeknob",
    Name = "Volume Knob",
    Icon = "Assets/Icons/volume.png",
    Tooltip = "Volume control with mute (short press) and reset (long press)",
    UserTitleEnabled = false,
    Controllers = ["Knob"],
    PropertyInspectorPath = "Assets/PropertyInspector/volumeknob.html"
)]
[SDActionState(Image = "Assets/Icons/volume.png")]
public class VolumeKnobHandler : ActionHandler
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(VolumeKnobHandler));

    private readonly object _lock = new();

    private bool _isMuted;
    private int _pendingVolumeChange;

    // Long press detection
    private DateTime? _pressStartTime;

    // Debounce timer for rotation
    private Timer? _updateTimer;

    // Volume state
    private int _volume;

    public VolumeKnobHandler(StreamDockConnection connection, string context, Dictionary<string, object>? settings)
        : base(connection, context, settings)
    {
        Logger.Info($"[VolumeKnob] Handler created - Context: {context}");
    }

    public override async Task OnWillAppearAsync()
    {
        Logger.Info($"[VolumeKnob] Action appeared - Context: {Context}");

        LoadSettings();

        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Load settings into internal state
    /// </summary>
    private void LoadSettings()
    {
        _volume = GetSetting("volume", 50);
        _isMuted = GetSetting("isMuted", false);

        Logger.Info($"[VolumeKnob] Settings loaded - Volume: {_volume}%, Muted: {_isMuted}");
    }

    /// <summary>
    ///     Called when settings are changed from Property Inspector
    /// </summary>
    public override async Task OnSettingsChangedAsync(Dictionary<string, object> settings)
    {
        Logger.Info("[VolumeKnob] Settings changed");

        // Update internal settings reference
        UpdateSettings(settings);

        // Reload settings into state
        LoadSettings();

        // Update display with new values
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Handle dial rotation - adjust volume with debouncing
    /// </summary>
    public override Task OnDialRotateAsync(int ticks, bool pressed)
    {
        Logger.Debug($"[VolumeKnob] Rotate - Ticks: {ticks}, Pressed: {pressed}");

        lock (_lock)
        {
            // Accumulate changes
            _pendingVolumeChange += ticks * 2; // 2 units per tick for smoother control

            // Cancel existing timer
            _updateTimer?.Dispose();

            // Set new debounce timer (30ms for responsive feel)
            _updateTimer = new Timer(_ => ApplyVolumeChange(), null, 30, Timeout.Infinite);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handle dial press down - start long press timer
    /// </summary>
    public override Task OnDialDownAsync()
    {
        Logger.Info("[VolumeKnob] Dial pressed down");

        lock (_lock)
        {
            _pressStartTime = DateTime.Now;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handle dial press up - determine short vs long press
    /// </summary>
    public override async Task OnDialUpAsync()
    {
        DateTime? startTime;

        lock (_lock)
        {
            startTime = _pressStartTime;
            _pressStartTime = null;
        }

        if (startTime == null) return;

        var pressDuration = (DateTime.Now - startTime.Value).TotalMilliseconds;
        var longPressThreshold = GetSetting("longPressThreshold", 500);

        Logger.Info($"[VolumeKnob] Dial released - Duration: {pressDuration}ms, Threshold: {longPressThreshold}ms");

        if (pressDuration >= longPressThreshold)
            // Long press - reset to default volume
            await HandleLongPress();
        else
            // Short press - toggle mute
            await HandleShortPress();
    }

    /// <summary>
    ///     Short press - toggle mute
    /// </summary>
    private async Task HandleShortPress()
    {
        Logger.Info("[VolumeKnob] Short press detected - Toggling mute");

        _isMuted = !_isMuted;

        await SaveStateAsync();
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Long press - reset to default volume
    /// </summary>
    private async Task HandleLongPress()
    {
        var defaultVolume = GetSetting("defaultVolume", 50);
        Logger.Info($"[VolumeKnob] Long press detected - Resetting to {defaultVolume}%");

        _volume = defaultVolume;
        _isMuted = false;

        await SaveStateAsync();
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Apply accumulated volume changes (debounced)
    /// </summary>
    private void ApplyVolumeChange()
    {
        int change;

        lock (_lock)
        {
            if (Math.Abs(_pendingVolumeChange) < 1) return;

            change = _pendingVolumeChange;
            _pendingVolumeChange = 0;
        }

        // Apply change and clamp
        _volume += change;
        _volume = Math.Clamp(_volume, 0, 100);

        Logger.Debug($"[VolumeKnob] Volume changed to {_volume}%");

        // Fire and forget update
        _ = Task.Run(async () =>
        {
            await Task.Delay(10); // Small delay for smoothness
            await SaveStateAsync();
            await UpdateDisplayAsync();
        });
    }

    /// <summary>
    ///     Update display with current volume and mute state
    /// </summary>
    public override async Task UpdateDisplayAsync()
    {
        try
        {
            // Set title
            var title = _isMuted ? "MUTED" : $"{_volume}%";
            await SetTitleAsync(title);

            // Set feedback
            await Connection.SetFeedbackAsync(Context, new Dictionary<string, object>
            {
                { "value", _volume },
                {
                    "indicator", new Dictionary<string, object>
                    {
                        { "value", _volume },
                        { "enabled", true }
                    }
                }
            });

            Logger.Debug($"[VolumeKnob] Display updated - Volume: {_volume}%, Muted: {_isMuted}");
        }
        catch (Exception ex)
        {
            Logger.Error("[VolumeKnob] Error updating display", ex);
        }
    }

    /// <summary>
    ///     Save current state to settings
    /// </summary>
    private async Task SaveStateAsync()
    {
        var newSettings = new Dictionary<string, object>(Settings ?? new Dictionary<string, object>())
        {
            ["volume"] = _volume,
            ["isMuted"] = _isMuted
        };

        await SetSettingsAsync(newSettings);
    }

    /// <summary>
    ///     Dispose resources
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            lock (_lock)
            {
                _updateTimer?.Dispose();
                _updateTimer = null;
            }

        base.Dispose(disposing);
    }
}