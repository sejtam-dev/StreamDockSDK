using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using log4net;
using StreamDockSDK.Events;
using StreamDockSDK.Models;
using StreamDockSDK.Models.Payloads;

namespace StreamDockSDK;

/// <summary>
///     Main connection class for StreamDock plugin communication
///     Similar to Elgato Stream Deck SDK
/// </summary>
public class StreamDockConnection : IDisposable
{
    private static readonly ILog log = LogManager.GetLogger(typeof(StreamDockConnection));
    private readonly PluginInfo _pluginInfo;
    private readonly string _pluginUuid;
    private readonly int _port;
    private readonly string _registerEvent;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _receiveTask;
    private ClientWebSocket? _webSocket;

    /// <summary>
    ///     Initialize StreamDock connection from command line arguments
    ///     Usage: plugin.exe -port 12345 -pluginUUID com.example.plugin -registerEvent registerPlugin -info {json}
    /// </summary>
    public StreamDockConnection(string[] args)
    {
        _port = ParsePort(args);
        _pluginUuid = ParsePluginUuid(args);
        _registerEvent = ParseRegisterEvent(args);
        _pluginInfo = ParsePluginInfo(args);
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _receiveTask?.Wait(TimeSpan.FromSeconds(2));
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }

    // Events similar to Stream Deck SDK
    // Using AsyncEventHandler for proper async/await support

    // Connection events (sync only - no async needed)
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    // Action lifecycle events
    public event AsyncEventHandler<WillAppearEventArgs>? WillAppear;
    public event AsyncEventHandler<WillDisappearEventArgs>? WillDisappear;

    // Key events
    public event AsyncEventHandler<KeyDownEventArgs>? KeyDown;
    public event AsyncEventHandler<KeyUpEventArgs>? KeyUp;

    // Dial/Knob events
    public event AsyncEventHandler<DialRotateEventArgs>? DialRotate;
    public event AsyncEventHandler<DialDownEventArgs>? DialDown;
    public event AsyncEventHandler<DialUpEventArgs>? DialUp;

    // Settings events
    public event AsyncEventHandler<DidReceiveSettingsEventArgs>? DidReceiveSettings;
    public event AsyncEventHandler<DidReceiveGlobalSettingsEventArgs>? DidReceiveGlobalSettings;

    // Title events
    public event AsyncEventHandler<TitleParametersDidChangeEventArgs>? TitleParametersDidChange;

    // Device events
    public event AsyncEventHandler<DeviceDidConnectEventArgs>? DeviceDidConnect;
    public event AsyncEventHandler<DeviceDidDisconnectEventArgs>? DeviceDidDisconnect;

    // Application events
    public event AsyncEventHandler<ApplicationDidLaunchEventArgs>? ApplicationDidLaunch;
    public event AsyncEventHandler<ApplicationDidTerminateEventArgs>? ApplicationDidTerminate;

    // System events
    public event AsyncEventHandler<EventArgs>? SystemDidWakeUp;

    // Property inspector events
    public event AsyncEventHandler<PropertyInspectorDidAppearEventArgs>? PropertyInspectorDidAppear;
    public event AsyncEventHandler<PropertyInspectorDidDisappearEventArgs>? PropertyInspectorDidDisappear;
    public event AsyncEventHandler<SendToPluginEventArgs>? SendToPlugin;

    /// <summary>
    ///     Connect to StreamDock WebSocket server
    /// </summary>
    public async Task ConnectAsync()
    {
        _webSocket = new ClientWebSocket();
        _cancellationTokenSource = new CancellationTokenSource();

        var uri = new Uri($"ws://localhost:{_port}");
        await _webSocket.ConnectAsync(uri, _cancellationTokenSource.Token);

        // Register plugin
        await RegisterPluginAsync();

        Connected?.Invoke(this, EventArgs.Empty);

        // Start receiving messages
        _receiveTask = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token));
    }

    /// <summary>
    ///     Register plugin with StreamDock
    /// </summary>
    private async Task RegisterPluginAsync()
    {
        var registration = new RegistrationInfo
        {
            Event = _registerEvent,
            Uuid = _pluginUuid
        };

        await SendAsync(registration);
    }

    /// <summary>
    ///     Receive and process messages from StreamDock
    /// </summary>
    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        var messageBuffer = new StringBuilder();

        try
        {
            while (_webSocket != null && _webSocket.State == WebSocketState.Open &&
                   !cancellationToken.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    break;
                }

                messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    var message = messageBuffer.ToString();
                    messageBuffer.Clear();

                    // Fire and forget - process message asynchronously
                    _ = ProcessMessageAsync(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            log.Error("Error receiving message", ex);
        }
    }

    /// <summary>
    ///     Process incoming message from StreamDock
    /// </summary>
    private async Task ProcessMessageAsync(string message)
    {
        try
        {
            // LOG RAW JSON MESSAGE - This is crucial for debugging!
            log.Debug($"[RAW JSON] {message}");

            var eventData = JsonSerializer.Deserialize<StreamDockEvent>(message);
            if (eventData == null) return;

            log.Debug(
                $"[StreamDock] Received event: {eventData.Event}, Action: {eventData.Action}, Context: {eventData.Context}");

            switch (eventData.Event)
            {
                case StreamDockEvents.KeyDown:
                    await (KeyDown?.Invoke(
                        this,
                        CreateKeyDownEventArgs(eventData)
                    ) ?? Task.CompletedTask);

                    break;

                case StreamDockEvents.KeyUp:
                    await (KeyUp?.Invoke(
                        this,
                        CreateKeyUpEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.DialRotate:
                    await (DialRotate?.Invoke(
                        this,
                        CreateDialRotateEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.DialDown:
                    await (DialDown?.Invoke(
                        this,
                        CreateDialDownEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.DialUp:
                    await (DialUp?.Invoke(
                        this,
                        CreateDialUpEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.WillAppear:
                    await (WillAppear?.Invoke(
                        this,
                        CreateWillAppearEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.WillDisappear:
                    await (WillDisappear?.Invoke(
                        this,
                        CreateWillDisappearEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.DidReceiveSettings:
                    await (DidReceiveSettings?.Invoke(
                        this,
                        CreateDidReceiveSettingsEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.DidReceiveGlobalSettings:
                    await (DidReceiveGlobalSettings?.Invoke(
                        this,
                        CreateDidReceiveGlobalSettingsEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.TitleParametersDidChange:
                    await (TitleParametersDidChange?.Invoke(
                        this,
                        CreateTitleParametersDidChangeEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.PropertyInspectorDidAppear:
                    await (PropertyInspectorDidAppear?.Invoke(
                        this,
                        CreatePropertyInspectorDidAppearEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.PropertyInspectorDidDisappear:
                    await (PropertyInspectorDidDisappear?.Invoke(
                        this,
                        CreatePropertyInspectorDidDisappearEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.SendToPlugin:
                    await (SendToPlugin?.Invoke(
                        this,
                        CreateSendToPluginEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.DeviceDidConnect:
                    await (DeviceDidConnect?.Invoke(
                        this,
                        CreateDeviceDidConnectEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.DeviceDidDisconnect:
                    await (DeviceDidDisconnect?.Invoke(
                        this,
                        CreateDeviceDidDisconnectEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.ApplicationDidLaunch:
                    await (ApplicationDidLaunch?.Invoke(
                        this,
                        CreateApplicationDidLaunchEventArgs(eventData)
                    ) ?? Task.CompletedTask);
                    break;

                case StreamDockEvents.ApplicationDidTerminate:
                    await (ApplicationDidTerminate?.Invoke(
                        this,
                        CreateApplicationDidTerminateEventArgs(eventData)
                    ) ?? Task.CompletedTask);

                    break;

                case StreamDockEvents.SystemDidWakeUp:
                    await (SystemDidWakeUp?.Invoke(
                        this,
                        EventArgs.Empty
                    ) ?? Task.CompletedTask);

                    break;
            }
        }
        catch (Exception ex)
        {
            log.Error("Error processing message", ex);
        }
    }

    // Helper methods to create typed event args

    private KeyDownEventArgs CreateKeyDownEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<KeyPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new KeyPayload();

        return new KeyDownEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new KeyPayload()
        };
    }

    private KeyUpEventArgs CreateKeyUpEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<KeyPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new KeyPayload();

        return new KeyUpEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new KeyPayload()
        };
    }

    private DialDownEventArgs CreateDialDownEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<DialPressPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new DialPressPayload();

        return new DialDownEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new DialPressPayload()
        };
    }

    private DialUpEventArgs CreateDialUpEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<DialPressPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new DialPressPayload();

        return new DialUpEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new DialPressPayload()
        };
    }

    private DialRotateEventArgs CreateDialRotateEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<DialRotatePayload>(JsonSerializer.Serialize(eventData.Payload))
            : new DialRotatePayload();

        return new DialRotateEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new DialRotatePayload()
        };
    }

    private WillAppearEventArgs CreateWillAppearEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<AppearancePayload>(JsonSerializer.Serialize(eventData.Payload))
            : new AppearancePayload();

        return new WillAppearEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new AppearancePayload()
        };
    }

    private WillDisappearEventArgs CreateWillDisappearEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<AppearancePayload>(JsonSerializer.Serialize(eventData.Payload))
            : new AppearancePayload();

        return new WillDisappearEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new AppearancePayload()
        };
    }

    private DidReceiveSettingsEventArgs CreateDidReceiveSettingsEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<SettingsPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new SettingsPayload();

        return new DidReceiveSettingsEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new SettingsPayload()
        };
    }

    private DidReceiveGlobalSettingsEventArgs CreateDidReceiveGlobalSettingsEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<GlobalSettingsPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new GlobalSettingsPayload();

        return new DidReceiveGlobalSettingsEventArgs
        {
            Payload = payload ?? new GlobalSettingsPayload()
        };
    }

    private TitleParametersDidChangeEventArgs CreateTitleParametersDidChangeEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<TitleParametersPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new TitleParametersPayload();

        return new TitleParametersDidChangeEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload ?? new TitleParametersPayload()
        };
    }

    private PropertyInspectorDidAppearEventArgs CreatePropertyInspectorDidAppearEventArgs(StreamDockEvent eventData)
    {
        return new PropertyInspectorDidAppearEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty
        };
    }

    private PropertyInspectorDidDisappearEventArgs CreatePropertyInspectorDidDisappearEventArgs(
        StreamDockEvent eventData)
    {
        return new PropertyInspectorDidDisappearEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty
        };
    }

    private SendToPluginEventArgs CreateSendToPluginEventArgs(StreamDockEvent eventData)
    {
        JsonElement payload;
        if (eventData.Payload != null)
        {
            var jsonString = JsonSerializer.Serialize(eventData.Payload);
            payload = JsonSerializer.Deserialize<JsonElement>(jsonString);
        }
        else
        {
            payload = JsonSerializer.SerializeToElement(new { });
        }

        return new SendToPluginEventArgs
        {
            Context = eventData.Context,
            Action = eventData.Action ?? string.Empty,
            Device = eventData.Device ?? string.Empty,
            Payload = payload
        };
    }

    private DeviceDidConnectEventArgs CreateDeviceDidConnectEventArgs(StreamDockEvent eventData)
    {
        var deviceInfo = eventData.DeviceInfo != null
            ? JsonSerializer.Deserialize<DeviceInfo>(JsonSerializer.Serialize(eventData.DeviceInfo))
            : null;

        return new DeviceDidConnectEventArgs
        {
            Device = eventData.Device ?? string.Empty,
            DeviceInfo = deviceInfo
        };
    }

    private DeviceDidDisconnectEventArgs CreateDeviceDidDisconnectEventArgs(StreamDockEvent eventData)
    {
        return new DeviceDidDisconnectEventArgs
        {
            Device = eventData.Device ?? string.Empty
        };
    }

    private ApplicationDidLaunchEventArgs CreateApplicationDidLaunchEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<ApplicationPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new ApplicationPayload();

        return new ApplicationDidLaunchEventArgs
        {
            Payload = payload ?? new ApplicationPayload()
        };
    }

    private ApplicationDidTerminateEventArgs CreateApplicationDidTerminateEventArgs(StreamDockEvent eventData)
    {
        var payload = eventData.Payload != null
            ? JsonSerializer.Deserialize<ApplicationPayload>(JsonSerializer.Serialize(eventData.Payload))
            : new ApplicationPayload();

        return new ApplicationDidTerminateEventArgs
        {
            Payload = payload ?? new ApplicationPayload()
        };
    }

    // Methods to send commands to StreamDock

    /// <summary>
    ///     Set the title of an action instance
    /// </summary>
    public async Task SetTitleAsync(string context, string title, int? state = null, string? target = null)
    {
        var message = new
        {
            @event = "setTitle",
            context,
            payload = new
            {
                title,
                target = target ?? "both",
                state
            }
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Set the image of an action instance
    /// </summary>
    public async Task SetImageAsync(string context, string? image, int? state = null, string? target = null)
    {
        var message = new
        {
            @event = "setImage",
            context,
            payload = new
            {
                image,
                target = target ?? "both",
                state
            }
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Show alert (yellow triangle) on action
    /// </summary>
    public async Task ShowAlertAsync(string context)
    {
        var message = new
        {
            @event = "showAlert",
            context
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Show OK (green checkmark) on action
    /// </summary>
    public async Task ShowOkAsync(string context)
    {
        var message = new
        {
            @event = "showOk",
            context
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Set the state of an action (for toggle buttons)
    /// </summary>
    public async Task SetStateAsync(string context, int state)
    {
        var message = new
        {
            @event = "setState",
            context,
            payload = new { state }
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Set settings for an action instance
    /// </summary>
    public async Task SetSettingsAsync(string context, Dictionary<string, object> settings)
    {
        var message = new
        {
            @event = "setSettings",
            context,
            payload = settings
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Get settings for an action instance
    /// </summary>
    public async Task GetSettingsAsync(string context)
    {
        var message = new
        {
            @event = "getSettings",
            context
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Set global settings for the plugin
    /// </summary>
    public async Task SetGlobalSettingsAsync(Dictionary<string, object> settings)
    {
        var message = new
        {
            @event = "setGlobalSettings",
            context = _pluginUuid,
            payload = settings
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Get global settings for the plugin
    /// </summary>
    public async Task GetGlobalSettingsAsync()
    {
        var message = new
        {
            @event = "getGlobalSettings",
            context = _pluginUuid
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Open a URL in the default browser
    /// </summary>
    public async Task OpenUrlAsync(string url)
    {
        var message = new
        {
            @event = "openUrl",
            payload = new { url }
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Log a message to StreamDock log
    /// </summary>
    public async Task LogMessageAsync(string logMessage)
    {
        var message = new
        {
            @event = "logMessage",
            payload = new { message = logMessage }
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Send message to property inspector
    /// </summary>
    public async Task SendToPropertyInspectorAsync(string context, object payload)
    {
        var message = new
        {
            @event = "sendToPropertyInspector",
            context,
            payload
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Set feedback for dial/encoder (display text/image)
    /// </summary>
    public async Task SetFeedbackAsync(string context, Dictionary<string, object> feedback)
    {
        var message = new
        {
            @event = "setFeedback",
            context,
            payload = feedback
        };

        await SendAsync(message);
    }

    /// <summary>
    ///     Generic send method
    /// </summary>
    private async Task SendAsync(object message)
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            return;

        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);

        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    // Parse command line arguments (Elgato style)
    private static int ParsePort(string[] args)
    {
        var portIndex = Array.IndexOf(args, "-port");
        if (portIndex >= 0 && portIndex + 1 < args.Length)
            if (int.TryParse(args[portIndex + 1], out var port))
                return port;
        throw new ArgumentException("Missing or invalid -port argument");
    }

    private static string ParsePluginUuid(string[] args)
    {
        var uuidIndex = Array.IndexOf(args, "-pluginUUID");
        if (uuidIndex >= 0 && uuidIndex + 1 < args.Length)
            return args[uuidIndex + 1];
        throw new ArgumentException("Missing -pluginUUID argument");
    }

    private static string ParseRegisterEvent(string[] args)
    {
        var registerIndex = Array.IndexOf(args, "-registerEvent");
        if (registerIndex >= 0 && registerIndex + 1 < args.Length)
            return args[registerIndex + 1];
        return "registerPlugin";
    }

    private static PluginInfo ParsePluginInfo(string[] args)
    {
        var infoIndex = Array.IndexOf(args, "-info");
        if (infoIndex >= 0 && infoIndex + 1 < args.Length)
            try
            {
                return JsonSerializer.Deserialize<PluginInfo>(args[infoIndex + 1]) ?? new PluginInfo();
            }
            catch
            {
                // Ignore parsing errors
            }

        return new PluginInfo();
    }
}