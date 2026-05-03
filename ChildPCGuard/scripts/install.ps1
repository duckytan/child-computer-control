# ChildPCGuard Installation Script
# Must be run as Administrator

param(
    [string]$Password = "admin"
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

Write-Host "ChildPCGuard Installation Started..." -ForegroundColor Green

# Paths
$installDir = "C:\Program Files\ChildPCGuard"
$system32 = $env:SystemRoot + "\System32"
$dataDir = "C:\ProgramData\ChildPCGuard"

# Create directories
Write-Host "Creating directories..."
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}
if (-not (Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
}
if (-not (Test-Path "$dataDir\logs")) {
    New-Item -ItemType Directory -Path "$dataDir\logs" -Force | Out-Null
}
if (-not (Test-Path "$dataDir\data")) {
    New-Item -ItemType Directory -Path "$dataDir\data" -Force | Out-Null
}

# Copy files
Write-Host "Copying files..."
$srcDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Copy-Item "$srcDir\src\ChildPCGuard.GuardService\bin\Release\net8.0\publish\*" $installDir -Force -Recurse -ErrorAction SilentlyContinue
Copy-Item "$srcDir\src\ChildPCGuard.LockOverlay\bin\Release\net8.0-windows\publish\LockOverlay.exe" "$system32\LockOverlay.exe" -Force

# Generate encryption key
$encryptionKey = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object { [char]$_ })

# Hash password
$hashedPassword = & "$installDir\ChildPCGuard.Shared.dll"

# Create config
Write-Host "Creating configuration..."
$config = @{
    Version = "1.0"
    IsEnabled = $true
    AdminPasswordHash = $encryptionKey
    Rules = @{
        Weekdays = @{
            DailyLimitMinutes = 120
            AllowedTimeWindows = @(
                @{Start = "15:00"; End = "20:00"}
            )
        }
        Weekends = @{
            DailyLimitMinutes = 240
            AllowedTimeWindows = @(
                @{Start = "09:00"; End = "21:00"}
            )
        }
    }
    AutoShutdownTime = "22:00"
    WarningMinutes = @(10, 5, 1)
    IdleThresholdMs = 5000
    ContinuousLimitMinutes = 45
    RestDurationMinutes = 5
    BlockedApps = @()
    BlockedSites = @()
    UseNtpValidation = $true
    NtpServers = @("pool.ntp.org", "time.windows.com", "cn.pool.ntp.org")
    NtpToleranceMinutes = 5
    ServiceName = "WinSecSvc_a1b2c3d4"
    ServiceDisplayName = "Windows Security Update Service"
    LockScreenMessage = "今天的使用时间已到，休息一下吧！"
    EmergencyUnlockShortcut = "Ctrl+Alt+Shift+F12"
}

$configJson = $config | ConvertTo-Json -Depth 10
Set-Content -Path "$dataDir\config.json" -Value $configJson

# Install service
Write-Host "Installing Windows Service..."
$serviceName = "WinSecSvc_a1b2c3d4"

# Stop if exists
$existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($existingService) {
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $serviceName
    Start-Sleep -Seconds 2
}

# Create service
$exePath = "$installDir\ChildPCGuard.GuardService.exe"
sc.exe create $serviceName binPath= "$exePath" start= auto DisplayName= "Windows Security Update Service"

# Set service recovery options
sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/60000/restart/60000

# Set ACL on directories
Write-Host "Setting permissions..."
$acl = Get-Acl $dataDir
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("SYSTEM", "FullControl", "Allow")
$acl.AddAccessRule($rule)
Set-Acl -Path $dataDir -AclObject $acl

# Start service
Write-Host "Starting service..."
Start-Service -Name $serviceName

Write-Host "Installation completed successfully!" -ForegroundColor Green
Write-Host "Service Name: $serviceName"
Write-Host "Data Directory: $dataDir"
