# StreamDock Manifest Builder

Automatic manifest.json generator for StreamDock plugins using C# attributes.

## Overview

The ManifestBuilder automatically generates `manifest.json` files for your StreamDock plugins by reading metadata from
C# attributes applied to your plugin and action classes. This eliminates the need to manually maintain JSON manifest
files and ensures consistency between your code and manifest.

## Installation

When you install StreamDockSDK via NuGet, ManifestBuilder is automatically included and configured:

```bash
dotnet add package StreamDockSDK
```

**That's it!** No additional configuration needed. The MSBuild targets are automatically imported and ManifestBuilder
will run after each build.

## How It Works

When you build your plugin project:

1. **MSBuild target runs** after compilation
2. **ManifestBuilder scans** your compiled assembly for StreamDock attributes
3. **Manifest is generated** in your output directory (`bin/Debug/net9.0/manifest.json`)
4. **If manifest exists**, generation is skipped (delete it to regenerate)

The ManifestBuilder is included in the StreamDockSDK NuGet package in the `tools/net9.0/` folder and is automatically
invoked by MSBuild targets.

## Usage

### 1. Apply Attributes to Your Plugin Class

```csharp
using StreamDockSDK;
using StreamDockSDK.Attributes;

[SDPlugin(
    PackageId = "com.example.myplugin",
    Name = "My Plugin",
    Version = "1.0.0",
    Author = "Your Name",
    Description = "Description of your plugin",
    Icon = "Assets/Icons/plugin.png",
    Category = "Custom Category",
    CategoryIcon = "Assets/Icons/category.png",
    CodePath = "MyPlugin.exe",
    SdkVersion = 1
)]
[SDPluginOS(Platform = "windows", MinimumVersion = "10")]
[SDPluginOS(Platform = "mac", MinimumVersion = "10.15")]
public class MyPlugin : StreamDockPlugin
{
    // Your plugin code
}
```

### 2. Apply Attributes to Action Handlers

```csharp
using StreamDockSDK.Actions;
using StreamDockSDK.Attributes;

[SDAction(
    Uuid = "myaction",  // Will become "com.example.myplugin.myaction"
    Name = "My Action",
    Icon = "Assets/Icons/action.png",
    Tooltip = "Does something awesome",
    Controllers = ["Keypad", "Knob"],
    PropertyInspectorPath = "Assets/PropertyInspector/action.html",
    UserTitleEnabled = true
)]
[SDActionState(
    Image = "Assets/Icons/state0.png",
    Title = "OFF",
    TitleColor = "#ff0000"
)]
[SDActionState(
    Image = "Assets/Icons/state1.png",
    Title = "ON",
    TitleColor = "#00ff00"
)]
public class MyActionHandler : ActionHandler
{
    // Your action handler code
}
```

### 3. Build Your Project

```bash
dotnet build
```

The `manifest.json` will be automatically generated in your output directory.

## Available Attributes

### SDPlugin

Main plugin metadata:

| Property                   | Required | Description                                            |
|----------------------------|----------|--------------------------------------------------------|
| `PackageId`                | ✅        | Unique package identifier (e.g., "com.example.plugin") |
| `Name`                     | ✅        | Plugin display name                                    |
| `Version`                  | ✅        | Plugin version (e.g., "1.0.0")                         |
| `Author`                   | ✅        | Plugin author name                                     |
| `Description`              | ✅        | Plugin description                                     |
| `Icon`                     | ✅        | Path to plugin icon (128x128 recommended)              |
| `SdkVersion`               | ✅        | SDK version (currently 1)                              |
| `CodePath`                 | ❌        | Path to executable (auto-detected if not specified)    |
| `Category`                 | ❌        | Custom category name                                   |
| `CategoryIcon`             | ❌        | Path to category icon (48x48)                          |
| `Url`                      | ❌        | Plugin website URL                                     |
| `MinimumVersionOfSoftware` | ❌        | Minimum StreamDock version required                    |

### SDPluginOS

Operating system requirement (can be applied multiple times):

| Property         | Required | Description                                                    |
|------------------|----------|----------------------------------------------------------------|
| `Platform`       | ✅        | "windows" or "mac"                                             |
| `MinimumVersion` | ✅        | Minimum OS version (e.g., "10" for Windows, "10.15" for macOS) |

### SDPluginApplicationsToMonitor

Monitor application launch/termination events (can be applied multiple times):

| Property       | Required | Description                             |
|----------------|----------|-----------------------------------------|
| `OS`           | ✅        | "windows" or "mac"                      |
| `Applications` | ✅        | Comma-separated list of apps to monitor |

```csharp
[SDPluginApplicationsToMonitor(OS = "windows", Applications = "notepad.exe,calc.exe")]
[SDPluginApplicationsToMonitor(OS = "mac", Applications = "com.apple.Safari,com.apple.Mail")]
```

### SDAction

Action metadata:

| Property                  | Required | Description                                                               |
|---------------------------|----------|---------------------------------------------------------------------------|
| `Uuid`                    | ✅        | Action identifier (PackageId will be prepended automatically)             |
| `Name`                    | ✅        | Action display name                                                       |
| `Icon`                    | ✅        | Path to action icon (40x40 recommended)                                   |
| `Tooltip`                 | ❌        | Tooltip text                                                              |
| `Controllers`             | ❌        | Supported controllers: "Keypad", "Knob", "Information", "SecondaryScreen" |
| `PropertyInspectorPath`   | ❌        | Path to Property Inspector HTML                                           |
| `UserTitleEnabled`        | ❌        | Allow custom title (default: true)                                        |
| `SupportedInMultiActions` | ❌        | Can be used in multi-actions (default: false)                             |
| `State`                   | ❌        | Default state index                                                       |
| `VisibleInActionsList`    | ❌        | Show in actions list (default: true)                                      |

### SDActionState

Action state definition (can be applied multiple times for multi-state actions):

| Property         | Required | Description                                |
|------------------|----------|--------------------------------------------|
| `Image`          | ✅        | Path to state image                        |
| `Title`          | ❌        | Default title text                         |
| `ShowTitle`      | ❌        | Show title (default: true)                 |
| `TitleColor`     | ❌        | Title color (hex, e.g., "#ffffff")         |
| `TitleAlignment` | ❌        | "top", "bottom", "center", "middle"        |
| `FontFamily`     | ❌        | Font family name                           |
| `FontStyle`      | ❌        | "Regular", "Bold", "Italic", "Bold Italic" |
| `FontSize`       | ❌        | Font size                                  |
| `FontUnderline`  | ❌        | Underline title (default: false)           |

## Manual Usage

You can also run the ManifestBuilder manually:

```bash
dotnet StreamDockSDK.ManifestBuilder.dll <assembly-path> <output-path>
```

Example:

```bash
dotnet StreamDockSDK.ManifestBuilder.dll "MyPlugin.dll" "manifest.json"
```

## How Package ID Works

The `PackageId` in your `[SDPlugin]` attribute is automatically prepended to all action UUIDs:

```csharp
[SDPlugin(PackageId = "com.example.myplugin", ...)]
public class MyPlugin : StreamDockPlugin { }

[SDAction(Uuid = "counter", ...)]  // Becomes "com.example.myplugin.counter"
public class CounterHandler : ActionHandler { }
```

This ensures all your actions have consistent, unique identifiers.

## Regenerating the Manifest

The ManifestBuilder only generates `manifest.json` if it doesn't already exist. To regenerate:

1. Delete `bin/Debug/net9.0/manifest.json`
2. Run `dotnet build`

Or use `dotnet clean` followed by `dotnet build`.

## Integration with StreamDockSDK

When you reference StreamDockSDK via NuGet, the MSBuild targets are automatically imported:

```xml
<ItemGroup>
  <PackageReference Include="StreamDockSDK" Version="1.0.0" />
</ItemGroup>
```

The targets file will automatically:

- Locate the ManifestBuilder (from NuGet package `tools/net9.0/` folder)
- Check if manifest needs regeneration
- Generate manifest from your attributes
- Place it in your output directory

**No additional configuration required!**

### For Local Development

If you're developing StreamDockSDK locally (not via NuGet), the ManifestBuilder will be automatically found in the local
build output:

```xml
<ItemGroup>
  <ProjectReference Include="..\StreamDockSDK\StreamDockSDK.csproj"/>
</ItemGroup>
```

### NuGet Package Structure

The StreamDockSDK NuGet package includes:

```
StreamDockSDK.1.0.0.nupkg
├── build/
│   └── StreamDockSDK.targets          (MSBuild integration)
├── lib/
│   └── net9.0/
│       └── StreamDockSDK.dll           (Runtime SDK)
└── tools/
    └── net9.0/
        └── StreamDockSDK.ManifestBuilder.dll  (Manifest generator)
```

## Troubleshooting

### "ManifestBuilder not found" warning

Make sure StreamDockSDK.ManifestBuilder is built:

```bash
dotnet build StreamDockSDK.ManifestBuilder
```

### Manifest not regenerating

Delete the existing manifest and rebuild:

```bash
Remove-Item bin/Debug/net9.0/manifest.json
dotnet build
```

### No actions in manifest

Ensure your action handlers have the `[SDAction]` attribute applied.

### PackageId not being prepended

Make sure you set `PackageId` in your `[SDPlugin]` attribute.

## License

MIT License - See LICENSE file for details
