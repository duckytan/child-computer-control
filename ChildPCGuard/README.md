# ChildPCGuard

A Windows desktop application for parental control over children's computer usage time.

## Features

- **Time-based restrictions**: Limit daily and continuous computer usage
- **Mandatory rest breaks**: Enforce breaks after extended usage sessions
- **Application blacklist**: Block specific programs from running
- **Website blacklist**: Prevent access to inappropriate websites
- **Safe mode detection**: Automatically shut down if Windows boots in Safe Mode
- **NTP time validation**: Prevent time manipulation to bypass restrictions
- **Stealth operation**: Runs as Windows Service with disguised agent processes
- **Three-layer guardian**: Service + dual agents ensure process protection

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      ChildPCGuard                             │
├─────────────────────────────────────────────────────────────┤
│  GuardService          Windows Service (SYSTEM)              │
│  ├── TimeTracker       Usage time monitoring                │
│  ├── AppMonitor        Blacklist app detection              │
│  ├── WebMonitor        Blacklist site detection             │
│  ├── ProcessGuardian   Agent process management             │
│  ├── ShutdownScheduler Auto-shutdown scheduling             │
│  └── NtpValidator      Time tampering detection             │
├─────────────────────────────────────────────────────────────┤
│  AgentA ↔ AgentB        Dual processes (mutual monitoring) │
│  └── ProcessDisguiser   svchost.exe / RuntimeBroker.exe    │
├─────────────────────────────────────────────────────────────┤
│  LockOverlay            WPF lock screen (fullscreen)        │
└─────────────────────────────────────────────────────────────┘
```

## Requirements

- Windows 10/11 Professional
- .NET 8 SDK
- Administrator privileges (for installation)

## Quick Start

### Build

```bash
git clone https://github.com/duckytan/child-computer-control.git
cd child-computer-control/ChildPCGuard
dotnet restore
dotnet build
```

### Install

```powershell
cd scripts
.\install.ps1 -Password "your_admin_password"
```

### Uninstall

```powershell
cd scripts
.\uninstall.ps1 -Password "your_admin_password"
```

## Configuration

All configuration is stored in `C:\ProgramData\ChildPCGuard\config.json`:

```json
{
    "IsEnabled": true,
    "ContinuousLimitMinutes": 45,
    "RestDurationMinutes": 5,
    "AutoShutdownTime": "22:00",
    "Rules": {
        "Weekdays": {
            "DailyLimitMinutes": 120,
            "AllowedTimeWindows": [{ "Start": "15:00", "End": "20:00" }]
        },
        "Weekends": {
            "DailyLimitMinutes": 240,
            "AllowedTimeWindows": [{ "Start": "09:00", "End": "21:00" }]
        }
    },
    "BlockedApps": ["game.exe"],
    "BlockedSites": ["youtube.com"]
}
```

## Project Structure

```
ChildPCGuard/
├── src/
│   ├── ChildPCGuard.Shared/        Shared library (Win32 API, messages, models)
│   ├── ChildPCGuard.GuardService/  Windows Service (core monitoring engine)
│   ├── ChildPCGuard.Agent/         Dual agent processes (mutual monitoring)
│   └── ChildPCGuard.LockOverlay/   WPF lock screen
├── scripts/
│   ├── install.ps1                  Installation script
│   └── uninstall.ps1                Uninstallation script
└── docs/                           Engineering documentation
```

## How It Works

1. **Service** runs with SYSTEM privilege, monitors user activity
2. **Agents** (disguised as svchost.exe and RuntimeBroker.exe) monitor each other
3. When time limit is reached, **LockOverlay** displays fullscreen lock
4. Only administrator password can unlock (except for mandatory rest)

## Data Storage

| Path | Description |
|------|-------------|
| `C:\ProgramData\ChildPCGuard\` | Root data directory |
| `C:\ProgramData\ChildPCGuard\config.json` | Configuration file |
| `C:\ProgramData\ChildPCGuard\logs\` | Process logs |
| `C:\ProgramData\ChildPCGuard\data\` | Usage data |

## Development

### Debug Service (Console Mode)

```powershell
cd src/ChildPCGuard.GuardService
dotnet run -- --console
```

### Build Release

```bash
dotnet build -c Release
dotnet publish -c Release
```

## Documentation

- [Architecture](./docs/ARCHITECTURE.md) - System design and components
- [Interfaces](./docs/INTERFACES.md) - Named Pipe messages, CLI commands, Win32 API
- [Developer Guide](./docs/DEVELOPER_GUIDE.md) - Setup, workflow, coding standards

## License

MIT
