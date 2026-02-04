# Universal StreamDock Plugin Build Script
# Reads manifest.json to get plugin information

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Get script and project root directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "Building StreamDock Plugin..." -ForegroundColor Cyan
Write-Host "Project: $ProjectRoot" -ForegroundColor Gray

# Read manifest.json
$ManifestPath = Join-Path $ProjectRoot "manifest.json"
if (-not (Test-Path $ManifestPath)) {
    Write-Host "ERROR: manifest.json not found at: $ManifestPath" -ForegroundColor Red
    exit 1
}

try {
    $Manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
    $PluginName = $Manifest.Name
    $PluginVersion = $Manifest.Version
    Write-Host "Plugin: $PluginName v$PluginVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to parse manifest.json: $_" -ForegroundColor Red
    exit 1
}

# Change to project root
Push-Location $ProjectRoot

try {
    # Clean previous build
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    dotnet clean -c $Configuration --nologo -v minimal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Clean failed!" -ForegroundColor Red
        exit 1
    }

    # Build project
    Write-Host "Building $Configuration configuration..." -ForegroundColor Yellow
    dotnet build -c $Configuration --nologo -v minimal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed!" -ForegroundColor Red
        exit 1
    }

    $OutputPath = "bin\$Configuration\net9.0"
    Write-Host ""
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Output directory: $OutputPath" -ForegroundColor Cyan
    Write-Host ""
    
    # Show what was built
    $OutputFullPath = Join-Path $ProjectRoot $OutputPath
    if (Test-Path $OutputFullPath) {
        Write-Host "Built files:" -ForegroundColor Gray
        Get-ChildItem $OutputFullPath -File | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor Gray
        }
        
        # Show folders
        $Folders = Get-ChildItem $OutputFullPath -Directory
        if ($Folders) {
            Write-Host "Folders:" -ForegroundColor Gray
            $Folders | ForEach-Object {
                Write-Host "  - $($_.Name)\" -ForegroundColor Gray
            }
        }
    }
    
    Write-Host ""
    Write-Host "Run './Scripts/Install.ps1' to install to StreamDock" -ForegroundColor Yellow

} finally {
    Pop-Location
}

