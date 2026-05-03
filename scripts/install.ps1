# ChildPCGuard Installation Script
# Must be run as Administrator

param(
    [string]$Password = "admin",
    [switch]$KeepData
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

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  ChildPCGuard Installation Script" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Paths
$installDir = "C:\Program Files\ChildPCGuard"
$system32 = $env:SystemRoot + "\System32"
$dataDir = "C:\ProgramData\ChildPCGuard"
$serviceName = "WinSecSvc_a1b2c3d4"

# Function to check if .NET 8 is installed
function Check-DotNetRuntime {
    try {
        $dotnetVersion = dotnet --list-runtimes | Select-String "Microsoft.WindowsDesktop.App.*8\."
        if ($dotnetVersion) {
            return $true
        }
    }
    catch {
        return $false
    }
    return $false
}

if (-not (Check-DotNetRuntime)) {
    Write-Host "Warning: .NET 8 Desktop Runtime not detected!" -ForegroundColor Yellow
    Write-Host "Please install .NET 8 Desktop Runtime before continuing." -ForegroundColor Yellow
    Write-Host "Download: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    $continue = Read-Host "Continue anyway? (y/N)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
}

# Create directories
Write-Host "Creating directories..." -ForegroundColor White
try {
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
    Write-Host "  Directories created." -ForegroundColor Green
}
catch {
    Write-Host "  Error creating directories: $_" -ForegroundColor Red
    exit 1
}

# Copy files
Write-Host "Copying files..." -ForegroundColor White
$srcDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishDir = Join-Path $srcDir "..\publish"
$copySuccess = $true

try {
    if (Test-Path $publishDir) {
        Copy-Item "$publishDir\GuardService\*" $installDir -Force -Recurse -ErrorAction SilentlyContinue
        Copy-Item "$publishDir\LockOverlay\LockOverlay.exe" "$system32\LockOverlay.exe" -Force
        Copy-Item "$publishDir\Agent\Agent.exe" "$system32\svchost.exe" -Force
        Copy-Item "$publishDir\Agent\Agent.exe" "$system32\RuntimeBroker.exe" -Force
    }
    else {
        Write-Host "  Warning: Publish directory not found. Looking for build output..." -ForegroundColor Yellow
        $guardServiceBin = Join-Path $srcDir "..\src\ChildPCGuard.GuardService\bin\Release\net8.0-windows"
        $agentBin = Join-Path $srcDir "..\src\ChildPCGuard.Agent\bin\Release\net8.0"
        $lockOverlayBin = Join-Path $srcDir "..\src\ChildPCGuard.LockOverlay\bin\Release\net8.0-windows"

        if (Test-Path $guardServiceBin) {
            Copy-Item "$guardServiceBin\*" $installDir -Force -Recurse
        }
        else {
            Write-Host "  Error: GuardService build output not found!" -ForegroundColor Red
            $copySuccess = $false
        }

        if (Test-Path $agentBin) {
            Copy-Item "$agentBin\ChildPCGuard.Agent.exe" "$system32\svchost.exe" -Force
            Copy-Item "$agentBin\ChildPCGuard.Agent.exe" "$system32\RuntimeBroker.exe" -Force
        }
        else {
            Write-Host "  Error: Agent build output not found!" -ForegroundColor Red
            $copySuccess = $false
        }

        if (Test-Path $lockOverlayBin) {
            Copy-Item "$lockOverlayBin\ChildPCGuard.LockOverlay.exe" "$system32\LockOverlay.exe" -Force
        }
        else {
            Write-Host "  Error: LockOverlay build output not found!" -ForegroundColor Red
            $copySuccess = $false
        }
    }

    if (-not $copySuccess) {
        Write-Host "Error: Some files could not be copied. Please build the solution first." -ForegroundColor Red
        Write-Host "Run: dotnet build ChildPCGuard.sln -c Release" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "  Files copied successfully." -ForegroundColor Green
}
catch {
    Write-Host "  Error copying files: $_" -ForegroundColor Red
    exit 1
}

# Generate password hash
Write-Host "Generating password hash..." -ForegroundColor White
$passwordBytes = [System.Text.Encoding]::UTF8.GetBytes($Password)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hashBytes = $sha256.ComputeHash($passwordBytes)
$hashedPassword = [BitConverter]::ToString($hashBytes).Replace("-", "").ToLower()

# Create config
Write-Host "Creating configuration..." -ForegroundColor White
$configJson = @"
{
  "Version": "1.0",
  "IsEnabled": true,
  "AdminPasswordHash": "$hashedPassword",
  "Rules": {
    "Weekdays": {
      "DailyLimitMinutes": 120,
      "AllowedTimeWindows": [
        { "Start": "15:00", "End": "20:00" }
      ]
    },
    "Weekends": {
      "DailyLimitMinutes": 240,
      "AllowedTimeWindows": [
        { "Start": "09:00", "End": "21:00" }
      ]
    }
  },
  "AutoShutdownTime": "22:00",
  "WarningMinutes": [10, 5, 1],
  "IdleThresholdMs": 5000,
  "ContinuousLimitMinutes": 45,
  "RestDurationMinutes": 5,
  "BlockedApps": [],
  "BlockedSites": [],
  "UseNtpValidation": true,
  "NtpServers": ["pool.ntp.org", "time.windows.com", "cn.pool.ntp.org"],
  "NtpToleranceMinutes": 5,
  "ServiceName": "WinSecSvc_a1b2c3d4",
  "ServiceDisplayName": "Windows Security Update Service",
  "LockScreenMessage": "今天的使用时间已到，休息一下吧！",
  "EmergencyUnlockShortcut": "Ctrl+Alt+Shift+F12"
}
"@

try {
    Set-Content -Path "$dataDir\config.json" -Value $configJson -Encoding UTF8
    Write-Host "  Configuration created with default password: $Password" -ForegroundColor Green
}
catch {
    Write-Host "  Error creating configuration: $_" -ForegroundColor Red
    exit 1
}

# Stop existing service
Write-Host "Stopping existing service (if any)..." -ForegroundColor White
$existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($existingService) {
    try {
        Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
        sc.exe delete $serviceName | Out-Null
        Start-Sleep -Seconds 1
        Write-Host "  Existing service removed." -ForegroundColor Green
    }
    catch {
        Write-Host "  Warning: Could not remove existing service: $_" -ForegroundColor Yellow
    }
}

# Install service
Write-Host "Installing Windows Service..." -ForegroundColor White
$exePath = "$installDir\ChildPCGuard.GuardService.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "Error: Service executable not found at $exePath" -ForegroundColor Red
    exit 1
}

try {
    sc.exe create $serviceName binPath= "`"$exePath`"" start= auto DisplayName= "Windows Security Update Service" | Out-Null
    sc.exe failure $serviceName reset= 86400 actions= restart/5000/restart/5000/restart/5000 | Out-Null
    Write-Host "  Service installed successfully." -ForegroundColor Green
}
catch {
    Write-Host "  Error installing service: $_" -ForegroundColor Red
    exit 1
}

# Set permissions
Write-Host "Setting permissions..." -ForegroundColor White
try {
    $acl = Get-Acl $dataDir
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule("SYSTEM", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.AddAccessRule($rule)
    Set-Acl -Path $dataDir -AclObject $acl

    $acl = Get-Acl $installDir
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule("SYSTEM", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.AddAccessRule($rule)
    Set-Acl -Path $installDir -AclObject $acl

    Write-Host "  Permissions set." -ForegroundColor Green
}
catch {
    Write-Host "  Warning: Could not set permissions: $_" -ForegroundColor Yellow
}

# Start service
Write-Host "Starting service..." -ForegroundColor White
try {
    Start-Service -Name $serviceName
    Start-Sleep -Seconds 2
    $serviceStatus = Get-Service -Name $serviceName
    if ($serviceStatus.Status -eq "Running") {
        Write-Host "  Service started successfully." -ForegroundColor Green
    }
    else {
        Write-Host "  Warning: Service started but status is: $($serviceStatus.Status)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  Error starting service: $_" -ForegroundColor Red
    Write-Host "  You can try starting it manually: Start-Service -Name $serviceName" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Installation Completed!" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Service Name: $serviceName"
Write-Host "Install Directory: $installDir"
Write-Host "Data Directory: $dataDir"
Write-Host "Default Password: $Password"
Write-Host ""
Write-Host "To uninstall, run: .\uninstall.ps1" -ForegroundColor Yellow
Write-Host ""
