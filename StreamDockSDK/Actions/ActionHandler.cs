using System.Text.Json;

namespace StreamDockSDK.Actions;

/// <summary>
///     Base class for handling StreamDock actions
///     Provides common functionality for managing action state and responding to events
///     Supports both synchronous and asynchronous programming models
/// </summary>
public abstract class ActionHandler(
    StreamDockConnection connection,
    string context,
    Dictionary<string, object>? settings) : IDisposable
{
    private bool _disposed;
    protected StreamDockConnection Connection { get; } = connection;
    protected string Context { get; } = context;
    protected Dictionary<string, object>? Settings { get; private set; } = settings;

    /// <summary>
    ///     Dispose of resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        Dispose(true);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Override to dispose of custom resources
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        // Override in derived classes to cleanup resources
    }

    /// <summary>
    ///     Updates the handler's settings
    /// </summary>
    public virtual void UpdateSettings(Dictionary<string, object>? settings)
    {
        Settings = settings;
    }

    /// <summary>
    ///     Called when settings are changed
    ///     Default implementation updates settings and refreshes display
    /// </summary>
    public virtual async Task OnSettingsChangedAsync(Dictionary<string, object> settings)
    {
        UpdateSettings(settings);
        await UpdateDisplayAsync();
    }

    /// <summary>
    ///     Called when the action appears on the device
    ///     Override to initialize the action
    /// </summary>
    public virtual Task OnWillAppearAsync()
    {
        return UpdateDisplayAsync();
    }

    /// <summary>
    ///     Called when the action disappears from the device
    ///     Override to cleanup resources
    /// </summary>
    public virtual Task OnWillDisappearAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Updates the display/visual representation of the action
    ///     Override to implement custom display logic
    /// </summary>
    public abstract Task UpdateDisplayAsync();

    /// <summary>
    ///     Called when a key is pressed down
    /// </summary>
    public virtual Task OnKeyDownAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called when a key is released
    /// </summary>
    public virtual Task OnKeyUpAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called when a dial/knob is rotated
    /// </summary>
    /// <param name="ticks">Number of ticks rotated (positive = clockwise, negative = counter-clockwise)</param>
    /// <param name="pressed">Whether the dial was pressed during rotation</param>
    public virtual Task OnDialRotateAsync(int ticks, bool pressed)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called when a dial/knob is pressed down
    /// </summary>
    public virtual Task OnDialDownAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called when a dial/knob is released
    /// </summary>
    public virtual Task OnDialUpAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called when Property Inspector sends data to the plugin
    ///     Override to handle custom messages from Property Inspector
    /// </summary>
    /// <param name="payload">Custom data sent from Property Inspector</param>
    public virtual Task OnSendToPluginAsync(JsonElement payload)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets a setting value with type conversion support
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="key">Setting key</param>
    /// <param name="defaultValue">Default value if setting not found or conversion fails</param>
    /// <returns>Setting value or default</returns>
    protected T? GetSetting<T>(string key, T? defaultValue = default)
    {
        if (Settings != null && Settings.TryGetValue(key, out var value))
            try
            {
                // Handle JsonElement type (from System.Text.Json)
                if (value is JsonElement jsonElement)
                {
                    // Convert JsonElement to target type
                    if (typeof(T) == typeof(string)) return (T?)(object?)jsonElement.GetString();

                    if (typeof(T) == typeof(int))
                    {
                        if (jsonElement.ValueKind == JsonValueKind.Number) return (T?)(object?)jsonElement.GetInt32();

                        if (jsonElement.ValueKind == JsonValueKind.String)
                            // Try parse string to int
                            if (int.TryParse(jsonElement.GetString(), out var intValue))
                                return (T?)(object?)intValue;
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        return (T?)(object?)jsonElement.GetBoolean();
                    }
                    else if (typeof(T) == typeof(float))
                    {
                        return (T?)(object?)jsonElement.GetSingle();
                    }
                    else if (typeof(T) == typeof(double))
                    {
                        return (T?)(object?)jsonElement.GetDouble();
                    }
                }

                // Handle direct type match
                if (value is T typedValue)
                    return typedValue;

                // Try convert
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                // Failed to convert, return default value
                return defaultValue;
            }

        return defaultValue;
    }

    /// <summary>
    ///     Set the title of this action instance
    /// </summary>
    protected Task SetTitleAsync(string title, int? state = null, string? target = null)
    {
        return Connection.SetTitleAsync(Context, title, state, target);
    }

    /// <summary>
    ///     Set the image of this action instance (base64 encoded)
    /// </summary>
    protected Task SetImageAsync(string? image, int? state = null, string? target = null)
    {
        return Connection.SetImageAsync(Context, image, state, target);
    }

    /// <summary>
    ///     Set the image from a file path (automatically converts to base64)
    /// </summary>
    /// <param name="filePath">Path to image file (PNG, JPG, GIF)</param>
    /// <param name="state">Optional state to set image for</param>
    /// <param name="target">Optional target (hardware/software)</param>
    /// <returns>Task</returns>
    protected async Task SetImageFromFileAsync(string filePath, int? state = null, string? target = null)
    {
        try
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"Image file not found: {filePath}");

            var imageBytes = await File.ReadAllBytesAsync(filePath);
            var base64Image = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
            await SetImageAsync(base64Image, state, target);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load image from file: {filePath}", ex);
        }
    }

    /// <summary>
    ///     Set settings for this action instance
    /// </summary>
    protected Task SetSettingsAsync(Dictionary<string, object> settings)
    {
        UpdateSettings(settings);
        return Connection.SetSettingsAsync(Context, settings);
    }

    /// <summary>
    ///     Set the state of this action (for toggle buttons)
    /// </summary>
    protected Task SetStateAsync(int state)
    {
        return Connection.SetStateAsync(Context, state);
    }

    /// <summary>
    ///     Show alert (yellow triangle) on this action
    /// </summary>
    protected Task ShowAlertAsync()
    {
        return Connection.ShowAlertAsync(Context);
    }

    /// <summary>
    ///     Show OK (green checkmark) on this action
    /// </summary>
    protected Task ShowOkAsync()
    {
        return Connection.ShowOkAsync(Context);
    }
}