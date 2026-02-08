using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using StreamDockSDK.Attributes;

namespace StreamDockSDK.ManifestBuilder;

/// <summary>
///     Manifest builder for StreamDock plugins.
///     Scans assemblies for StreamDock attributes and generates manifest.json.
/// </summary>
public class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: StreamDockSDK.ManifestBuilder <assembly-path> <output-path>");
                Console.Error.WriteLine("  assembly-path: Path to the compiled plugin assembly (DLL)");
                Console.Error.WriteLine("  output-path: Path where manifest.json will be generated");
                return 1;
            }

            var assemblyPath = args[0];
            var outputPath = args[1];

            if (!File.Exists(assemblyPath))
            {
                Console.Error.WriteLine($"Error: Assembly not found at '{assemblyPath}'");
                return 1;
            }

            Console.WriteLine($"Loading assembly from: {assemblyPath}");
            Console.WriteLine($"Output manifest to: {outputPath}");

            // Load the assembly
            var assembly = Assembly.LoadFrom(assemblyPath);

            // Generate manifest
            var manifest = GenerateManifest(assembly);

            // Serialize to JSON
            var json = JsonSerializer.Serialize(manifest, JsonOptions);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir)) Directory.CreateDirectory(outputDir);

            // Write manifest file
            File.WriteAllText(outputPath, json);

            Console.WriteLine($"✓ Manifest generated successfully: {outputPath}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error generating manifest: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static Dictionary<string, object> GenerateManifest(Assembly assembly)
    {
        // Find the plugin class (class with SDPluginAttribute)
        var pluginType = assembly.GetTypes()
            .FirstOrDefault(t => t.GetCustomAttribute<SDPluginAttribute>() != null);

        if (pluginType == null)
            throw new InvalidOperationException(
                "No class with [SDPlugin] attribute found in assembly. " +
                "Please add [SDPlugin] attribute to your main plugin class.");

        var pluginAttr = pluginType.GetCustomAttribute<SDPluginAttribute>()!;

        // Extract package ID from plugin attribute
        var packageId = pluginAttr.PackageId;

        // Build manifest dictionary
        var manifest = new Dictionary<string, object>();

        // Find all action classes
        var actionTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<SDActionAttribute>() != null)
            .ToList();

        if (actionTypes.Count == 0)
            throw new InvalidOperationException(
                "No classes with [SDAction] attribute found. " +
                "At least one action is required.");

        // Build Actions array
        var actions = new List<Dictionary<string, object>>();
        foreach (var actionType in actionTypes)
        {
            var actionAttr = actionType.GetCustomAttribute<SDActionAttribute>()!;
            var action = BuildAction(actionAttr, actionType, packageId);
            actions.Add(action);
        }

        manifest["Actions"] = actions;

        // Add plugin metadata
        manifest["Version"] = pluginAttr.Version;
        manifest["Name"] = pluginAttr.Name;
        manifest["Icon"] = pluginAttr.Icon;
        manifest["Description"] = pluginAttr.Description;
        manifest["Author"] = pluginAttr.Author;
        manifest["SDKVersion"] = pluginAttr.SdkVersion;

        // Determine CodePath (auto-detect or use provided)
        if (!string.IsNullOrEmpty(pluginAttr.CodePath))
        {
            manifest["CodePath"] = pluginAttr.CodePath;
        }
        else
        {
            // Auto-detect: if assembly name ends with .exe, use it, otherwise assume .dll
            var assemblyName = assembly.GetName().Name ?? "plugin";
            manifest["CodePath"] = assemblyName + ".exe";
        }

        // Optional fields
        if (!string.IsNullOrEmpty(pluginAttr.Category))
            manifest["Category"] = pluginAttr.Category;

        if (!string.IsNullOrEmpty(pluginAttr.CategoryIcon))
            manifest["CategoryIcon"] = pluginAttr.CategoryIcon;

        if (!string.IsNullOrEmpty(pluginAttr.CodePathMac))
            manifest["CodePathMac"] = pluginAttr.CodePathMac;

        if (!string.IsNullOrEmpty(pluginAttr.CodePathWin))
            manifest["CodePathWin"] = pluginAttr.CodePathWin;

        if (!string.IsNullOrEmpty(pluginAttr.PropertyInspectorPath))
            manifest["PropertyInspectorPath"] = pluginAttr.PropertyInspectorPath;

        if (!string.IsNullOrEmpty(pluginAttr.Url))
            manifest["URL"] = pluginAttr.Url;

        // Build OS array
        var osAttrs = pluginType.GetCustomAttributes<SDPluginOSAttribute>().ToList();
        if (osAttrs.Count > 0)
        {
            var osList = osAttrs.Select(os => new Dictionary<string, object>
            {
                ["Platform"] = os.Platform,
                ["MinimumVersion"] = os.MinimumVersion
            }).ToList();
            manifest["OS"] = osList;
        }

        // Software minimum version
        if (!string.IsNullOrEmpty(pluginAttr.MinimumVersionOfSoftware))
            manifest["Software"] = new Dictionary<string, object>
            {
                ["MinimumVersion"] = pluginAttr.MinimumVersionOfSoftware
            };

        // Applications to monitor
        var appsToMonitorAttrs = pluginType.GetCustomAttributes<SDPluginApplicationsToMonitorAttribute>().ToList();
        if (appsToMonitorAttrs.Count > 0)
        {
            var appsDict = new Dictionary<string, object>();
            foreach (var attr in appsToMonitorAttrs) appsDict[attr.OS] = attr.Applications;
            manifest["ApplicationsToMonitor"] = appsDict;
        }

        return manifest;
    }

    private static Dictionary<string, object> BuildAction(SDActionAttribute actionAttr, Type actionType,
        string? packageId)
    {
        var action = new Dictionary<string, object>();

        // Build UUID (prepend package ID if provided)
        var uuid = actionAttr.Uuid;
        if (!string.IsNullOrEmpty(packageId) && !uuid.StartsWith(packageId)) uuid = $"{packageId}.{uuid}";
        action["UUID"] = uuid;

        action["Icon"] = actionAttr.Icon;
        action["Name"] = actionAttr.Name;

        if (actionAttr.State.HasValue)
            action["State"] = actionAttr.State.Value;

        // Build States array
        var stateAttrs = actionType.GetCustomAttributes<SDActionStateAttribute>().ToList();
        if (stateAttrs.Count > 0)
        {
            var states = stateAttrs.Select(state => BuildState(state)).ToList();
            action["States"] = states;
        }
        else
        {
            // If no states defined, create a default one with the action icon
            action["States"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["Image"] = actionAttr.Icon
                }
            };
        }

        if (!string.IsNullOrEmpty(actionAttr.PropertyInspectorPath))
            action["PropertyInspectorPath"] = actionAttr.PropertyInspectorPath;

        action["SupportedInMultiActions"] = actionAttr.SupportedInMultiActions;

        if (!string.IsNullOrEmpty(actionAttr.Tooltip))
            action["Tooltip"] = actionAttr.Tooltip;

        if (actionAttr.Settings != null && actionAttr.Settings.Count > 0)
            action["Settings"] = actionAttr.Settings;

        action["UserTitleEnabled"] = actionAttr.UserTitleEnabled;

        if (actionAttr.Controllers != null && actionAttr.Controllers.Length > 0)
            action["Controllers"] = actionAttr.Controllers;

        action["VisibleInActionsList"] = actionAttr.VisibleActionsList;

        if (actionAttr.Os != null && actionAttr.Os.Length > 0)
            action["OS"] = actionAttr.Os;

        return action;
    }

    private static Dictionary<string, object> BuildState(SDActionStateAttribute state)
    {
        var stateDict = new Dictionary<string, object>
        {
            ["Image"] = state.Image
        };

        if (!string.IsNullOrEmpty(state.Title))
            stateDict["Title"] = state.Title;

        stateDict["ShowTitle"] = state.ShowTitle;

        if (!string.IsNullOrEmpty(state.TitleColor))
            stateDict["TitleColor"] = state.TitleColor;

        if (!string.IsNullOrEmpty(state.TitleAlignment))
            stateDict["TitleAlignment"] = state.TitleAlignment;

        if (!string.IsNullOrEmpty(state.FontFamily))
            stateDict["FontFamily"] = state.FontFamily;

        if (!string.IsNullOrEmpty(state.FontStyle))
            stateDict["FontStyle"] = state.FontStyle;

        if (state.FontSize.HasValue)
            stateDict["FontSize"] = state.FontSize.Value;

        stateDict["FontUnderline"] = state.FontUnderline;

        return stateDict;
    }
}