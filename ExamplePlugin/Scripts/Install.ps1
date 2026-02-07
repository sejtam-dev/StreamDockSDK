# Universal StreamDock Plugin Install Script
# Reads manifest.json to determine plugin ID and installs to StreamDock

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Get script and project root directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "Installing StreamDock Plugin..." -ForegroundColor Cyan
Write-Host "Project: $ProjectRoot" -ForegroundColor Gray

# Build path
$BuildPath = Join-Path $ProjectRoot "bin\$Configuration\net9.0"

# Read manifest.json from build output
$ManifestPath = Join-Path $BuildPath "manifest.json"
if (-not (Test-Path $ManifestPath)) {
    Write-Host "ERROR: manifest.json not found at: $ManifestPath" -ForegroundColor Red
    Write-Host "Run './Scripts/Build.ps1' first to generate manifest.json" -ForegroundColor Yellow
    exit 1
}

try {
    $Manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
    $PluginName = $Manifest.Name
    $PluginVersion = $Manifest.Version
    
    # Generate plugin folder name from first action UUID or use a default pattern
    if ($Manifest.Actions -and $Manifest.Actions.Count -gt 0) {
        $FirstActionUUID = $Manifest.Actions[0].UUID
        # Extract base domain from UUID (e.g., "com.example.counter" -> "com.example")
        $UUIDParts = $FirstActionUUID -split '\.'
        if ($UUIDParts.Count -ge 2) {
            $BaseDomain = ($UUIDParts[0..($UUIDParts.Count - 2)]) -join '.'
        } else {
            $BaseDomain = $FirstActionUUID
        }
        $PluginFolderName = "$BaseDomain.sdPlugin"
    } else {
        Write-Host "ERROR: No actions found in manifest.json" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Plugin: $PluginName v$PluginVersion" -ForegroundColor Green
    Write-Host "Plugin ID: $PluginFolderName" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to parse manifest.json: $_" -ForegroundColor Red
    exit 1
}


# Check if build exists
if (-not (Test-Path $BuildPath)) {
    Write-Host "Build not found at: $BuildPath" -ForegroundColor Red
    Write-Host "Run './Scripts/Build.ps1' first" -ForegroundColor Yellow
    exit 1
}

# StreamDock plugins path
$StreamDockPluginsPath = "$env:APPDATA\HotSpot\StreamDock\plugins"
$PluginInstallPath = Join-Path $StreamDockPluginsPath $PluginFolderName

Write-Host ""
Write-Host "Installation paths:" -ForegroundColor Gray
Write-Host "  Source: $BuildPath" -ForegroundColor Gray
Write-Host "  Target: $PluginInstallPath" -ForegroundColor Gray
Write-Host ""

# Create plugins directory if it doesn't exist
if (-not (Test-Path $StreamDockPluginsPath)) {
    Write-Host "Creating StreamDock plugins directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $StreamDockPluginsPath -Force | Out-Null
}

# Remove old plugin if exists
if (Test-Path $PluginInstallPath) {
    Write-Host "Removing old plugin installation..." -ForegroundColor Yellow
    try {
        Remove-Item -Path $PluginInstallPath -Recurse -Force -ErrorAction Stop
        Write-Host "Old plugin removed successfully" -ForegroundColor Green
    } catch {
        Write-Host "WARNING: Could not remove old plugin. StreamDock may be running." -ForegroundColor Yellow
        Write-Host "Error: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please close StreamDock and try again." -ForegroundColor Yellow
        exit 1
    }
}

# Copy plugin files (excluding .pdb files)
Write-Host "Copying plugin files..." -ForegroundColor Yellow
try {
    # Create destination directory
    New-Item -ItemType Directory -Path $PluginInstallPath -Force | Out-Null
    
    # Copy all files except .pdb
    Get-ChildItem -Path $BuildPath -Recurse | ForEach-Object {
        $DestPath = $_.FullName.Replace($BuildPath, $PluginInstallPath)
        
        if ($_.PSIsContainer) {
            # Create directory
            if (-not (Test-Path $DestPath)) {
                New-Item -ItemType Directory -Path $DestPath -Force | Out-Null
            }
        } elseif ($_.Extension -ne ".pdb") {
            # Copy file (skip .pdb files)
            Copy-Item -Path $_.FullName -Destination $DestPath -Force
        }
    }
    
    Write-Host "Plugin files copied successfully (excluding .pdb files)" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to copy plugin files: $_" -ForegroundColor Red
    exit 1
}

# Verify installation
if (Test-Path $PluginInstallPath) {
    $InstalledFiles = Get-ChildItem $PluginInstallPath -Recurse -File
    $FileCount = $InstalledFiles.Count
    $FolderCount = (Get-ChildItem $PluginInstallPath -Recurse -Directory).Count
    
    Write-Host ""
    Write-Host "Plugin installed successfully!" -ForegroundColor Green
    Write-Host "Location: $PluginInstallPath" -ForegroundColor Cyan
    Write-Host "Files: $FileCount, Folders: $FolderCount" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Start or restart StreamDock application" -ForegroundColor White
    Write-Host "  2. The plugin should appear in the actions list" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "ERROR: Installation verification failed!" -ForegroundColor Red
    exit 1
}


