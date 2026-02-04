using log4net;

namespace ExamplePlugin;

/// <summary>
///     Entry point for the Example Plugin
/// </summary>
internal class Program
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

    public static async Task Main(string[] args)
    {
        Logger.Info("Example StreamDock Plugin starting...");

        var plugin = new ExamplePlugin();
        await plugin.RunAsync(args);

        Logger.Info("Example StreamDock Plugin shutting down");
    }
}