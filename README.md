# ChildPCGuard

儿童电脑使用时间控制系统，用于家长管理孩子使用电脑的时间。

## 功能特性

- **时长限制**: 限制每日和连续使用电脑的时间
- **强制休息**: 连续使用后自动进入休息模式
- **程序黑名单**: 禁止运行指定的程序
- **网站黑名单**: 阻止访问指定的网站
- **安全模式检测**: 检测到安全模式启动时自动关机
- **NTP 时间验证**: 防止通过修改系统时间绕过限制
- **隐蔽运行**: 作为 Windows 服务运行，Agent 进程伪装成系统进程
- **三层守护**: 服务 + 双 Agent 确保进程保护

## 系统架构

```
┌─────────────────────────────────────────────────────────────┐
│                      ChildPCGuard                             │
├─────────────────────────────────────────────────────────────┤
│  GuardService          Windows 服务 (SYSTEM 权限)             │
│  ├── TimeTracker       使用时间监控                          │
│  ├── AppMonitor        黑名单程序检测                        │
│  ├── WebMonitor        黑名单网站检测                        │
│  ├── ProcessGuardian   Agent 进程管理                        │
│  ├── ShutdownScheduler 自动关机调度                          │
│  └── NtpValidator      时间篡改检测                          │
├─────────────────────────────────────────────────────────────┤
│  AgentA ↔ AgentB        双进程互相监控                        │
│  └── ProcessDisguiser   伪装为 svchost.exe / RuntimeBroker.exe │
├─────────────────────────────────────────────────────────────┤
│  LockOverlay            WPF 锁屏界面 (全屏)                   │
└─────────────────────────────────────────────────────────────┘
```

## 系统要求

- Windows 10/11 专业版
- .NET 8 SDK
- 管理员权限 (用于安装)

## 快速开始

### 编译

```bash
git clone https://github.com/duckytan/child-computer-control.git
cd child-computer-control/ChildPCGuard
dotnet restore
dotnet build
```

### 安装

```powershell
cd scripts
.\install.ps1 -Password "your_admin_password"
```

### 卸载

```powershell
cd scripts
.\uninstall.ps1 -Password "your_admin_password"
```

## 配置说明

配置文件位于 `C:\ProgramData\ChildPCGuard\config.json`:

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

## 项目结构

```
ChildPCGuard/
├── src/
│   ├── ChildPCGuard.Shared/        共享库 (Win32 API, 消息, 数据模型)
│   ├── ChildPCGuard.GuardService/  Windows 服务 (核心监控引擎)
│   ├── ChildPCGuard.Agent/         双 Agent 进程 (互相监控)
│   └── ChildPCGuard.LockOverlay/   WPF 锁屏界面
├── scripts/
│   ├── install.ps1                  安装脚本
│   └── uninstall.ps1                卸载脚本
└── docs/                           工程文档
```

## 工作原理

1. **服务** 以 SYSTEM 权限运行，监控用户活动
2. **Agent** (伪装成 svchost.exe 和 RuntimeBroker.exe) 互相监控
3. 当时间限制到达时，**LockOverlay** 显示全屏锁屏
4. 只有管理员密码可以解锁 (强制休息期间除外)

## 数据存储

| 路径 | 说明 |
|------|------|
| `C:\ProgramData\ChildPCGuard\` | 根数据目录 |
| `C:\ProgramData\ChildPCGuard\config.json` | 配置文件 |
| `C:\ProgramData\ChildPCGuard\logs\` | 进程日志 |
| `C:\ProgramData\ChildPCGuard\data\` | 使用数据 |

## 开发调试

### 服务控制台调试

```powershell
cd src/ChildPCGuard.GuardService
dotnet run -- --console
```

### 发布 Release 版本

```bash
dotnet build -c Release
dotnet publish -c Release
```

## 文档

- [架构文档](./docs/ARCHITECTURE.md) - 系统设计和组件
- [接口文档](./docs/INTERFACES.md) - 命名管道消息、CLI 命令、Win32 API
- [开发者指南](./docs/DEVELOPER_GUIDE.md) - 环境搭建、工作流程、编码规范

## 开源协议

MIT
