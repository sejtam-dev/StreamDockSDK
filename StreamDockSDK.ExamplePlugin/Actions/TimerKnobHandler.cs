using System.Text.Json;
using log4net;
using StreamDockSDK;
using StreamDockSDK.Actions;
using StreamDockSDK.Attributes;

namespace ExamplePlugin.Actions;

/// <summary>
///     Timer Knob action demonstrating practical encoder usage
///     Features:
///     - Rotate to set timer duration (1-60 minutes)
///     - Short press: Start/Stop timer
///     - Long press: Reset timer
///     - Shows remaining time and progress
///     - Alert when timer completes
///     Settings:
///     - duration (int): Timer duration in minutes (1-60)
///     - isRunning (bool): Timer running state
///     - startTime (string): ISO timestamp when timer started
/// </summary>
[SDAction(
    Uuid = "timerknob",
    Name = "Timer Knob",
    Icon = "Assets/Icons/timer.png",
    Tooltip = "Countdown timer (1-60 minutes)",
    UserTitleEnabled = false,
    Controllers = [
        "Knob"
    ],
    PropertyInspectorPath = "Assets/PropertyInspector/timerknob.html"
)]
[SDActionState(Image = "Assets/Icons/timer.png")]
public class TimerKnobHandler : ActionHandler
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(TimerKnobHandler));

    private readonly object _lock = new();

    // Update timer for running countdown
    private Timer? _countdownTimer;

    // Timer state
    private int _durationMinutes;
    private bool _isRunning;

    // Long press detection
    private DateTime? _pressStartTime;
    private DateTime? _startTime;

    public TimerKnobHandler(StreamDockConnection connection, string context, Dictionary<string, object>? settings)
        : base(connection, context, settings)
    {
        Logger.Info($"[TimerKnob] Handler created - Context: {context}");
    }

    public override async Task OnWillAppearAsync()
    {
        Logger.Info($"[TimerKnob] Action appeared - Context: {Context}");

        LoadSettings();

        // Resume countdown if was running
        if (_isRunning && _startTime.HasValue) StartCountdown();

        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Load settings into internal state
    /// </summary>
    private void LoadSettings()
    {
        _durationMinutes = GetSetting("duration", 5);
        _isRunning = GetSetting("isRunning", false);

        var startTimeStr = GetSetting("startTime", string.Empty);
        if (!string.IsNullOrEmpty(startTimeStr) && DateTime.TryParse(startTimeStr, out var startTime))
            _startTime = startTime;

        Logger.Info($"[TimerKnob] Settings loaded - Duration: {_durationMinutes}m, Running: {_isRunning}");
    }

    /// <summary>
    ///     Called when settings are changed from Property Inspector
    /// </summary>
    public override async Task OnSettingsChangedAsync(Dictionary<string, object> settings)
    {
        Logger.Info("[TimerKnob] Settings changed");

        // Update internal settings reference
        UpdateSettings(settings);

        // Reload settings into state
        var oldDuration = _durationMinutes;
        LoadSettings();

        // Only update display if duration changed and not running
        if (oldDuration != _durationMinutes && !_isRunning)
        {
            Logger.Info($"[TimerKnob] Duration changed from {oldDuration}m to {_durationMinutes}m");
            await UpdateDisplayAsync();
        }
    }

    /// <summary>
    ///     Handle custom messages from Property Inspector (e.g., reset button)
    /// </summary>
    public override async Task OnSendToPluginAsync(JsonElement payload)
    {
        Logger.Info("[TimerKnob] Received sendToPlugin message");

        // Check if this is a reset command
        if (payload.TryGetProperty("action", out var actionObj) && actionObj.GetString() == "resetTimer")
        {
            Logger.Info("[TimerKnob] Reset command received from Property Inspector");
            await HandleReset();
        }
    }

    /// <summary>
    ///     Handle dial rotation - adjust duration
    /// </summary>
    public override async Task OnDialRotateAsync(int ticks, bool pressed)
    {
        Logger.Debug($"[TimerKnob] Rotate - Ticks: {ticks}");

        // Can only adjust when not running
        if (_isRunning)
        {
            Logger.Info("[TimerKnob] Cannot adjust while running");
            return;
        }

        _durationMinutes += ticks;
        _durationMinutes = Math.Clamp(_durationMinutes, 1, 60);

        Logger.Info($"[TimerKnob] Duration set to {_durationMinutes} minutes");

        await SaveStateAsync();
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Handle dial press down
    /// </summary>
    public override Task OnDialDownAsync()
    {
        lock (_lock)
        {
            _pressStartTime = DateTime.Now;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handle dial press up - start/stop or reset
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

        if (pressDuration >= 500)
            // Long press - reset
            await HandleReset();
        else
            // Short press - start/stop
            await HandleStartStop();
    }

    /// <summary>
    ///     Start or stop the timer
    /// </summary>
    private async Task HandleStartStop()
    {
        if (_isRunning)
        {
            // Stop
            Logger.Info("[TimerKnob] Stopping timer");
            StopCountdown();
        }
        else
        {
            // Start
            Logger.Info($"[TimerKnob] Starting timer for {_durationMinutes} minutes");
            _startTime = DateTime.Now;
            _isRunning = true;
            StartCountdown();
        }

        await SaveStateAsync();
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Reset the timer
    /// </summary>
    private async Task HandleReset()
    {
        Logger.Info("[TimerKnob] Resetting timer");

        StopCountdown();
        _startTime = null;

        await SaveStateAsync();
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Start countdown timer
    /// </summary>
    private void StartCountdown()
    {
        lock (_lock)
        {
            _countdownTimer?.Dispose();
            _countdownTimer = new Timer(_ => UpdateCountdown(), null, 0, 1000); // Update every second
        }
    }

    /// <summary>
    ///     Stop countdown timer
    /// </summary>
    private void StopCountdown()
    {
        _isRunning = false;

        lock (_lock)
        {
            _countdownTimer?.Dispose();
            _countdownTimer = null;
        }
    }

    /// <summary>
    ///     Update countdown (called every second)
    /// </summary>
    private void UpdateCountdown()
    {
        if (!_isRunning || !_startTime.HasValue) return;

        var elapsed = DateTime.Now - _startTime.Value;
        var totalDuration = TimeSpan.FromMinutes(_durationMinutes);
        var remaining = totalDuration - elapsed;

        if (remaining.TotalSeconds <= 0)
        {
            // Timer finished!
            Logger.Info("[TimerKnob] Timer finished!");

            StopCountdown();

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                await SaveStateAsync();
                await UpdateDisplayAsync();
            });
        }
        else
        {
            // Update display
            _ = Task.Run(async () => await UpdateDisplayAsync());
        }
    }

    /// <summary>
    ///     Update display with current timer state
    /// </summary>
    public override async Task UpdateDisplayAsync()
    {
        try
        {
            string title;
            int progressPercent;

            if (_isRunning && _startTime.HasValue)
            {
                var elapsed = DateTime.Now - _startTime.Value;
                var totalDuration = TimeSpan.FromMinutes(_durationMinutes);
                var remaining = totalDuration - elapsed;

                if (remaining.TotalSeconds > 0)
                {
                    // Show remaining time
                    if (remaining.TotalMinutes >= 1)
                        title = $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2}";
                    else
                        title = $"{(int)remaining.TotalSeconds}s";

                    progressPercent = (int)(elapsed.TotalMilliseconds / totalDuration.TotalMilliseconds * 100);
                }
                else
                {
                    title = "DONE!";
                    progressPercent = 100;
                }
            }
            else
            {
                // Show set duration
                title = $"{_durationMinutes}m";
                progressPercent = 0;
            }

            await SetTitleAsync(title);

            // Set feedback with progress indicator
            await Connection.SetFeedbackAsync(Context, new Dictionary<string, object>
            {
                { "value", progressPercent },
                {
                    "indicator", new Dictionary<string, object>
                    {
                        { "value", progressPercent },
                        { "enabled", _isRunning }
                    }
                }
            });

            Logger.Debug($"[TimerKnob] Display updated - Title: {title}, Progress: {progressPercent}%");
        }
        catch (Exception ex)
        {
            Logger.Error("[TimerKnob] Error updating display", ex);
        }
    }

    /// <summary>
    ///     Save current state to settings
    /// </summary>
    private async Task SaveStateAsync()
    {
        var newSettings = new Dictionary<string, object>(Settings ?? new Dictionary<string, object>())
        {
            ["duration"] = _durationMinutes,
            ["isRunning"] = _isRunning,
            ["startTime"] = _startTime?.ToString("o") ?? ""
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
                _countdownTimer?.Dispose();
                _countdownTimer = null;
            }

        base.Dispose(disposing);
    }
}