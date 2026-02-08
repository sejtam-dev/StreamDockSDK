# Clean Build Artifacts Script
# Removes bin and obj folders

$ErrorActionPreference = "Stop"

# Get script and project root directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "Cleaning build artifacts..." -ForegroundColor Cyan
Write-Host "Project: $ProjectRoot" -ForegroundColor Gray
Write-Host ""

Push-Location $ProjectRoot

try {
    # Clean using dotnet
    Write-Host "Running dotnet clean..." -ForegroundColor Yellow
    dotnet clean --nologo -v minimal
    
    # Remove bin and obj folders
    $BinPath = Join-Path $ProjectRoot "bin"
    $ObjPath = Join-Path $ProjectRoot "obj"
    
    if (Test-Path $BinPath) {
        Write-Host "Removing bin folder..." -ForegroundColor Yellow
        Remove-Item -Path $BinPath -Recurse -Force
        Write-Host "  bin/ removed" -ForegroundColor Green
    }
    
    if (Test-Path $ObjPath) {
        Write-Host "Removing obj folder..." -ForegroundColor Yellow
        Remove-Item -Path $ObjPath -Recurse -Force
        Write-Host "  obj/ removed" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Clean completed successfully!" -ForegroundColor Green
    
} finally {
    Pop-Location
}

