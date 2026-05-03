# ChildPCGuard Uninstall Script
# Must be run as Administrator

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

# Paths
$installDir = "C:\Program Files\ChildPCGuard"
$system32 = $env:SystemRoot + "\System32"
$dataDir = "C:\ProgramData\ChildPCGuard"
$serviceName = "WinSecSvc_a1b2c3d4"

# Stop and delete service
Write-Host "Stopping and removing service..."
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($service) {
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    sc.exe delete $serviceName
    Write-Host "Service removed."
} else {
    Write-Host "Service not found."
}

# Kill running processes
Write-Host "Terminating running processes..."
Get-Process -Name "ChildPCGuard.GuardService" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "svchost" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "$system32\svchost.exe" -and $_.StartTime -gt (Get-Date).AddHours(-1) } | Stop-Process -Force
Get-Process -Name "RuntimeBroker" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "$system32\RuntimeBroker.exe" -and $_.StartTime -gt (Get-Date).AddHours(-1) } | Stop-Process -Force
Get-Process -Name "LockOverlay" -ErrorAction SilentlyContinue | Stop-Process -Force

# Delete installed files
Write-Host "Deleting installed files..."
if (Test-Path "$system32\svchost.exe") {
    Remove-Item "$system32\svchost.exe" -Force -ErrorAction SilentlyContinue
}
if (Test-Path "$system32\RuntimeBroker.exe") {
    Remove-Item "$system32\RuntimeBroker.exe" -Force -ErrorAction SilentlyContinue
}
if (Test-Path "$system32\LockOverlay.exe") {
    Remove-Item "$system32\LockOverlay.exe" -Force -ErrorAction SilentlyContinue
}

# Delete installation directory
if (Test-Path $installDir) {
    Remove-Item $installDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Installation directory removed."
}

# Delete data directory (optional, keep logs for audit)
Write-Host "Data directory ($dataDir) preserved for audit purposes."
Write-Host "To delete all data, run: Remove-Item '$dataDir' -Recurse -Force"

Write-Host "Uninstallation completed!" -ForegroundColor Green
