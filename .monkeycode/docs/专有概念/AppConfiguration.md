# AppConfiguration

AppConfiguration 是应用的主配置类，包含所有控制规则和参数。

## 什么是 AppConfiguration？

AppConfiguration 定义了系统的所有可配置参数，包括时间限制、黑名单、NTP 设置等。该类被序列化为 JSON 存储在 `config.json` 文件中，由 ConfigManager 组件负责加载和保存。

**关键特征**:
- 支持工作日和周末不同规则
- 可配置的程序和网站黑名单
- NTP 时间验证防篡改
- AES 加密密码存储

## 代码位置

| 方面 | 位置 |
|------|------|
| 定义 | `src/ChildPCGuard.Shared/Models.cs` |
| 序列化 | `src/ChildPCGuard.GuardService/ConfigManager.cs` |
| 使用 | 所有 GuardService 组件 |

## 结构

```csharp
public class AppConfiguration
{
    public string Version { get; set; } = "1.0";
    public bool IsEnabled { get; set; } = true;
    public string AdminPasswordHash { get; set; }
    public RulesConfiguration Rules { get; set; } = new RulesConfiguration();
    public string AutoShutdownTime { get; set; } = "22:00";
    public int[] WarningMinutes { get; set; } = new[] { 10, 5, 1 };
    public int IdleThresholdMs { get; set; } = 5000;
    public int ContinuousLimitMinutes { get; set; } = 45;
    public int RestDurationMinutes { get; set; } = 5;
    public List<string> BlockedApps { get; set; } = new List<string>();
    public List<string> BlockedSites { get; set; } = new List<string>();
    public bool UseNtpValidation { get; set; } = true;
    public string[] NtpServers { get; set; } = new[] { "pool.ntp.org", "time.windows.com" };
    public int NtpToleranceMinutes { get; set; } = 5;
    public string ServiceName { get; set; } = "WinSecSvc_a1b2c3d4";
    public string ServiceDisplayName { get; set; } = "Windows Security Update Service";
    public string LockScreenMessage { get; set; } = "今天的使用时间已到，休息一下吧！";
    public string EmergencyUnlockShortcut { get; set; } = "Ctrl+Alt+Shift+F12";
}
```

## 配置分类

### 时间控制

| 字段 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| `AutoShutdownTime` | string | "22:00" | 每日自动关机时间 |
| `ContinuousLimitMinutes` | int | 45 | 连续使用限制（分钟） |
| `RestDurationMinutes` | int | 5 | 强制休息时长（分钟） |
| `IdleThresholdMs` | int | 5000 | 空闲阈值（毫秒），低于此值认为用户在使用 |
| `WarningMinutes` | int[] | [10, 5, 1] | 剩余时间警告分钟数 |

### 规则配置

| 字段 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| `Rules.Weekdays.DailyLimitMinutes` | int | 120 | 工作日每日限制（分钟） |
| `Rules.Weekends.DailyLimitMinutes` | int | 240 | 周末每日限制（分钟） |
| `Rules.Weekdays.AllowedTimeWindows` | TimeWindow[] | 15:00-20:00 | 工作日允许时间段 |
| `Rules.Weekends.AllowedTimeWindows` | TimeWindow[] | 09:00-21:00 | 周末允许时间段 |

### 黑名单

| 字段 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| `BlockedApps` | List<string> | [] | 禁止运行的程序列表 |
| `BlockedSites` | List<string> | [] | 禁止访问的网站列表 |

### 安全设置

| 字段 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| `AdminPasswordHash` | string | "" | 家长密码（AES加密） |
| `UseNtpValidation` | bool | true | 是否启用NTP时间验证 |
| `NtpServers` | string[] | [3个NTP服务器] | NTP服务器列表 |
| `NtpToleranceMinutes` | int | 5 | NTP时间容差（分钟） |

## 配置文件示例

```json
{
    "Version": "1.0",
    "IsEnabled": true,
    "AdminPasswordHash": "encrypted_hash_here",
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
    "BlockedApps": ["game.exe"],
    "BlockedSites": ["youtube.com"],
    "UseNtpValidation": true,
    "NtpServers": ["pool.ntp.org", "time.windows.com"],
    "NtpToleranceMinutes": 5,
    "ServiceName": "WinSecSvc_a1b2c3d4",
    "ServiceDisplayName": "Windows Security Update Service",
    "LockScreenMessage": "今天的使用时间已到，休息一下吧！",
    "EmergencyUnlockShortcut": "Ctrl+Alt+Shift+F12"
}
```

## 相关组件

- [ConfigManager](./模块/GuardService.md#configmanager) - 配置加载和保存
- [TimeTracker](./TimeTracker.md) - 使用时间统计，使用规则配置
- [AppMonitor](./模块/GuardService.md#appmonitor) - 程序监控，使用黑名单
- [WebMonitor](./模块/GuardService.md#webmonitor) - 网站监控，使用黑名单
