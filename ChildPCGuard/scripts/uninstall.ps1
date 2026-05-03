# ChildPCGuard Uninstallation Script
# Must be run as Administrator

param(
    [string]$Password
)

$ErrorActionPreference = "Stop"

# Check admin rights
$currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Error: This script must be run as Administrator" -ForegroundColor Red
    exit 1
}

Write-Host "ChildPCGuard Uninstallation Started..." -ForegroundColor Yellow

# Verify password
if ($Password) {
    $configPath = "C:\ProgramData\ChildPCGuard\config.json"
    if (Test-Path $configPath) {
        $config = Get-Content $configPath | ConvertFrom-Json
        Write-Host "Password verification not implemented in this version"
    }
}

$serviceName = "WinSecSvc_a1b2c3d4"
$system32 = $env:SystemRoot + "\System32"
$installDir = "C:\Program Files\ChildPCGuard"
$dataDir = "C:\ProgramData\ChildPCGuard"

# Stop service
Write-Host "Stopping service..."
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($service) {
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
}

# Delete service
Write-Host "Deleting service..."
sc.exe delete $serviceName 2>$null

# Remove files from System32
Write-Host "Removing files from System32..."
$filesToRemove = @(
    "$system32\svchost.exe",
    "$system32\RuntimeBroker.exe",
    "$system32\LockOverlay.exe"
)

foreach ($file in $filesToRemove) {
    if (Test-Path $file) {
        Remove-Item $file -Force -ErrorAction SilentlyContinue
        Write-Host "Removed: $file"
    }
}

# Remove scheduled tasks
Write-Host "Removing scheduled tasks..."
Unregister-ScheduledTask -TaskName "ChildPCGuard_Backup" -Confirm:$false -ErrorAction SilentlyContinue

# Ask about data directory
$removeData = Read-Host "Do you want to remove data directory (C:\ProgramData\ChildPCGuard)? (yes/no)"
if ($removeData -eq "yes") {
    Write-Host "Removing data directory..."
    if (Test-Path $dataDir) {
        Remove-Item $dataDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# Remove installation directory
Write-Host "Removing installation directory..."
if (Test-Path $installDir) {
    Remove-Item $installDir -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Uninstallation completed!" -ForegroundColor Green
