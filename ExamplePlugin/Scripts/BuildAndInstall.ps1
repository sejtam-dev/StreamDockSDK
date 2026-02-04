# Quick Build and Install Script
# Combines Build.ps1 and Install.ps1 for convenience

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "=== Quick Build & Install ===" -ForegroundColor Cyan
Write-Host ""

# Run Build
Write-Host "Step 1: Building..." -ForegroundColor Yellow
& "$ScriptDir\Build.ps1" -Configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Build failed! Aborting installation." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Installing..." -ForegroundColor Yellow
& "$ScriptDir\Install.ps1" -Configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Installation failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Done! ===" -ForegroundColor Green

