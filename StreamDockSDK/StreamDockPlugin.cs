using System.Reflection;
using log4net;
using log4net.Config;
using StreamDockSDK.Actions;
using StreamDockSDK.Events;

namespace StreamDockSDK;

/// <summary>
///     Base class for StreamDock plugins
///     Simplifies plugin development similar to Elgato's approach
/// </summary>
public abstract class StreamDockPlugin : IDisposable
{
    protected static readonly ILog Logger =
        LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(StreamDockPlugin));

    protected StreamDockConnection Connection { get; private set; } = null!;

    /// <summary>
    ///     Action handler manager for automatic handler management
    ///     Use this to register handlers or factories in your plugin's constructor or OnConnected
    /// </summary>
    protected ActionHandlerManager HandlerManager { get; } = new();

    public virtual void Dispose()
    {
        HandlerManager?.Dispose();
        Connection?.Dispose();
    }

    /// <summary>
    ///     Initialize and run the plugin
    /// </summary>
    public async Task RunAsync(string[] args)
    {
        // Initialize log4net automatically
        InitializeLogging();

        Logger.Info($"StreamDock Plugin starting... (Type: {GetType().Name})");

        Connection = new StreamDockConnection(args);

        // Subscribe to events
        RegisterEventHandlers();

        // Connect to StreamDock
        await Connection.ConnectAsync();

        // Keep plugin running
        await Task.Delay(Timeout.Infinite);
    }

    /// <summary>
    ///     Initialize log4net logging
    /// </summary>
    private void InitializeLogging()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var logRepository = LogManager.GetRepository(assembly);
        var configFile = new FileInfo("log4net.config");

        if (configFile.Exists)
        {
            // Use plugin's custom log4net.config
            XmlConfigurator.Configure(logRepository, configFile);
            Logger.Debug($"log4net configured from: {configFile.FullName}");
        }
        else
        {
            // Use embedded default config from StreamDockSDK
            var resourceStream =
                typeof(StreamDockPlugin).Assembly.GetManifestResourceStream("StreamDockSDK.Assets.log4net.config");
            if (resourceStream != null)
            {
                XmlConfigurator.Configure(logRepository, resourceStream);
                Logger.Debug("log4net configured from embedded default config");
            }
            else
            {
                // Last resort - basic configuration
                Logger.Warn("No log4net.config found, using basic configuration");
                BasicConfigurator.Configure(logRepository);
            }
        }
    }

    /// <summary>
    ///     Register event handlers for StreamDock events
    /// </summary>
    public virtual void RegisterEventHandlers()
    {
        Connection.WillAppear += async (_, e) => await OnWillAppearAsync(e);
        Connection.WillDisappear += async (_, e) => await OnWillDisappearAsync(e);

        Connection.KeyDown += async (_, e) => await OnKeyDownAsync(e);
        Connection.KeyUp += async (_, e) => await OnKeyUpAsync(e);

        Connection.DialRotate += async (_, e) => await OnDialRotateAsync(e);
        Connection.DialDown += async (_, e) => await OnDialDownAsync(e);
        Connection.DialUp += async (_, e) => await OnDialUpAsync(e);

        Connection.DidReceiveSettings += async (_, e) => await OnDidReceiveSettingsAsync(e);
        Connection.SendToPlugin += async (_, e) => await OnSendToPluginAsync(e);
    }

    private async Task OnWillAppearAsync(WillAppearEventArgs e)
    {
        Logger.Debug($"[HandlerManager] OnWillAppear - Action: {e.Action}, Context: {e.Context}");

        var handler = HandlerManager.GetOrCreateHandler(e.Action, Connection, e.Context, e.GetSettings());
        if (handler != null)
        {
            Logger.Debug($"[HandlerManager] Handler created/retrieved for {e.Action}");
            await handler.OnWillAppearAsync();
        }
        else
        {
            Logger.Warn($"[HandlerManager] No handler registered for action: {e.Action}");
        }
    }


    private async Task OnWillDisappearAsync(WillDisappearEventArgs e)
    {
        Logger.Debug($"[HandlerManager] OnWillDisappear - Context: {e.Context}");

        var handler = HandlerManager.GetHandler(e.Context);
        if (handler != null) await handler.OnWillDisappearAsync();
        HandlerManager.RemoveHandler(e.Context);
    }

    private async Task OnKeyDownAsync(KeyDownEventArgs e)
    {
        Logger.Debug($"[HandlerManager] OnKeyDown - Context: {e.Context}");

        var handler = HandlerManager.GetHandler(e.Context);
        if (handler != null) await handler.OnKeyDownAsync();
    }

    private async Task OnKeyUpAsync(KeyUpEventArgs e)
    {
        Logger.Debug($"[HandlerManager] OnKeyUp - Context: {e.Context}");

        var handler = HandlerManager.GetHandler(e.Context);
        if (handler != null) await handler.OnKeyUpAsync();
    }

    private async Task OnDialRotateAsync(DialRotateEventArgs e)
    {
        Logger.Debug(
            $"[HandlerManager] OnDialRotate - Context: {e.Context}, Ticks: {e.GetTicks()}, Pressed: {e.IsPressed()}");

        var handler = HandlerManager.GetHandler(e.Context);
        if (handler != null) await handler.OnDialRotateAsync(e.GetTicks(), e.IsPressed());
    }

    private async Task OnDialDownAsync(DialDownEventArgs e)
    {
        Logger.Debug($"[HandlerManager] OnDialDown - Context: {e.Context}");

        var handler = HandlerManager.GetHandler(e.Context);
        if (handler != null) await handler.OnDialDownAsync();
    }


    private async Task OnDialUpAsync(DialUpEventArgs e)
    {
        Logger.Debug($"[HandlerManager] OnDialUp - Context: {e.Context}");

        var handler = HandlerManager.GetHandler(e.Context);
        if (handler != null) await handler.OnDialUpAsync();
    }

    private async Task OnDidReceiveSettingsAsync(DidReceiveSettingsEventArgs e)
    {
        Logger.Debug($"[HandlerManager] OnDidReceiveSettings - Context: {e.Context}");

        var handler = HandlerManager.GetHandler(e.Context);
        if (handler != null && e.GetSettings() != null) await handler.OnSettingsChangedAsync(e.GetSettings()!);
    }

    private async Task OnSendToPluginAsync(SendToPluginEventArgs e)
    {
        Logger.Debug($"[HandlerManager] OnSendToPlugin - Context: {e.Context}");

        var handler = HandlerManager.GetHandler(e.Context);
        if (handler != null) await handler.OnSendToPluginAsync(e.Payload);
    }
}