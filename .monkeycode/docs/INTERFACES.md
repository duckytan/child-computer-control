# 接口文档

## Named Pipe 消息接口

**管道名称**: `ChildPCGuardPipe`
**通信模式**: 命名管道服务器/客户端
**序列化**: BinaryFormatter

### PipeMessageType 枚举

| 值 | 名称 | 方向 | 描述 |
|----|------|------|------|
| 1 | Heartbeat | Agent → Service | 心跳消息 |
| 2 | LockRequest | Agent → Service | 锁屏请求 |
| 3 | UnlockRequest | Agent → Service | 解锁请求 |
| 4 | RestartAgent | Agent → Service | 重启Agent请求 |
| 5 | ConfigChanged | Service → Agent | 配置变更通知 |
| 6 | GetStatus | Agent → Service | 获取状态请求 |
| 7 | StatusResponse | Service → Agent | 状态响应 |
| 8 | LogRecord | Agent → Service | 日志记录 |
| 9 | AddTime | Agent → Service | 增加使用时间 |
| 10 | PauseControl | Agent → Service | 暂停控制 |
| 11 | LockNow | Agent/Service → Service | 立即锁屏 |
| 12 | ShutdownNow | Agent → Service | 立即关机 |

### 消息结构

#### PipeMessage (基类)

```csharp
[Serializable]
public class PipeMessage
{
    public PipeMessageType Type { get; set; }    // 消息类型
    public string ProcessName { get; set; }       // 进程名
    public int ProcessId { get; set; }            // 进程ID
    public DateTime Timestamp { get; set; }        // 时间戳
    public string Payload { get; set; }            // 载荷数据
}
```

#### HeartbeatMessage

```csharp
[Serializable]
public class HeartbeatMessage : PipeMessage
{
    public uint MemoryUsage { get; set; }         // 内存使用量
    public TimeSpan Uptime { get; set; }          // 运行时间
}
```

#### StatusMessage

```csharp
[Serializable]
public class StatusMessage : PipeMessage
{
    public UsageState CurrentState { get; set; }   // 当前状态
    public TimeSpan UsedTimeToday { get; set; }    // 今日使用时间
    public TimeSpan RemainingTime { get; set; }     // 剩余时间
    public TimeSpan ContinuousUsageTime { get; set; } // 连续使用时间
    public TimeSpan RestRemainingTime { get; set; } // 休息剩余时间
    public DateTime? ShutdownTime { get; set; }    // 关机时间
    public int BlockedAppsCount { get; set; }      // 黑名单程序数
    public int BlockedSitesCount { get; set; }     // 黑名单网站数
    public bool IsServiceRunning { get; set; }     // 服务运行状态
}
```

## 数据模型接口

### UsageState 枚举

```csharp
public enum UsageState
{
    Using = 0,      // 使用中
    Resting = 1,   // 强制休息中
    Locked = 2,    // 锁定
    Shutdown = 3,  // 关机
    Paused = 4,    // 暂停
    Normal = 10     // 正常
}
```

### LockReason 枚举

```csharp
public enum LockReason
{
    DailyLimitReached = 1,    // 每日限制到达
    ContinuousLimit = 2,      // 连续限制到达
    OutsideAllowedWindow = 3,  // 超出允许时间窗口
    TimeTampered = 4,          // 时间篡改
    ManualLock = 5,            // 手动锁屏
    AutoShutdown = 6,         // 自动关机
    BlockedApp = 7,           // 黑名单程序
    BlockedSite = 8,          // 黑名单网站
    SafeMode = 9              // 安全模式
}
```

### AppConfiguration 配置类

```csharp
public class AppConfiguration
{
    public string Version { get; set; }                      // 版本号
    public bool IsEnabled { get; set; }                     // 是否启用
    public string AdminPasswordHash { get; set; }          // 管理员密码哈希
    public RulesConfiguration Rules { get; set; }           // 规则配置
    public string AutoShutdownTime { get; set; }            // 自动关机时间
    public int[] WarningMinutes { get; set; }              // 警告分钟数
    public int IdleThresholdMs { get; set; }              // 空闲阈值(毫秒)
    public int ContinuousLimitMinutes { get; set; }       // 连续限制(分钟)
    public int RestDurationMinutes { get; set; }           // 休息时长(分钟)
    public List<string> BlockedApps { get; set; }          // 黑名单程序
    public List<string> BlockedSites { get; set; }        // 黑名单网站
    public bool UseNtpValidation { get; set; }           // 是否启用NTP验证
    public string[] NtpServers { get; set; }             // NTP服务器列表
    public int NtpToleranceMinutes { get; set; }           // NTP容差(分钟)
    public string ServiceName { get; set; }              // 服务名
    public string ServiceDisplayName { get; set; }        // 服务显示名
    public string LockScreenMessage { get; set; }          // 锁屏消息
    public string EmergencyUnlockShortcut { get; set; }    // 紧急解锁快捷键
}

public class RulesConfiguration
{
    public TimeRule Weekdays { get; set; }     // 工作日规则
    public TimeRule Weekends { get; set; }     // 周末规则
}

public class TimeRule
{
    public int DailyLimitMinutes { get; set; }               // 每日限制(分钟)
    public List<TimeWindow> AllowedTimeWindows { get; set; } // 允许时间窗口
}

public class TimeWindow
{
    public string Start { get; set; }   // 开始时间 "HH:mm"
    public string End { get; set; }     // 结束时间 "HH:mm"
}
```

## 命令行接口

### GuardService

```bash
# 控制台模式运行 (调试用)
ChildPCGuard.GuardService.exe --console

# Windows 服务模式 (默认)
ChildPCGuard.GuardService.exe
```

### Agent

```bash
# 启动 AgentA (伪装为 svchost.exe)
Agent.exe --agent-a

# 启动 AgentB (伪装为 RuntimeBroker.exe)
Agent.exe --agent-b
```

### LockOverlay

```bash
# 根据锁屏原因启动
LockOverlay.exe <LockReason>
```

## 安装脚本接口

### install.ps1

```powershell
# 基本安装 (使用默认密码 admin)
.\install.ps1

# 指定密码安装
.\install.ps1 -Password "your_password"
```

### uninstall.ps1

```powershell
# 基本卸载 (不删除数据)
.\uninstall.ps1

# 卸载并删除数据目录
.\uninstall.ps1 -Password "your_password"
```

## 配置 CLI (config.exe)

> 注: config.exe 在当前版本中未实现，以下为设计规格

```bash
# 查看当前配置
config.exe --view

# 修改每日时长限制
config.exe --set dailyLimit <minutes>

# 修改连续使用限制
config.exe --set continuousLimit <minutes>

# 修改休息时长
config.exe --set restDuration <minutes>

# 修改关机时间
config.exe --set shutdownTime "<HH:mm>"

# 添加程序到黑名单
config.exe --block-app <program.exe>

# 从黑名单移除程序
config.exe --unblock-app <program.exe>

# 添加网站到黑名单
config.exe --block-site <domain.com>

# 从黑名单移除网站
config.exe --unblock-site <domain.com>

# 查看今日日志
config.exe --view-logs today

# 重置当日使用时间
config.exe --reset-daily
```

## Win32 API 接口

### NativeAPI 主要方法

| 方法 | DLL | 描述 |
|------|-----|------|
| `LockWorkStation()` | user32.dll | 锁定工作站 |
| `GetLastInputInfo()` | user32.dll | 获取最后输入时间 |
| `CreateDesktop()` | user32.dll | 创建桌面 |
| `SwitchDesktop()` | user32.dll | 切换桌面 |
| `GetForegroundWindow()` | user32.dll | 获取前台窗口 |
| `GetWindowThreadProcessId()` | user32.dll | 获取窗口所属进程ID |
| `OpenProcess()` | kernel32.dll | 打开进程 |
| `TerminateProcess()` | kernel32.dll | 终止进程 |
| `CreateProcess()` | kernel32.dll | 创建进程 |

## 事件日志

**源名称**: 应用事件日志
**查看方式**: `Get-EventLog -LogName Application -Source WinSecSvc`

| 条目类型 | 描述 |
|----------|------|
| Information | 服务启动/停止、配置加载 |
| Warning | Agent 心跳超时、进程退出、检测到时间篡改 |
| Error | 服务启动失败、锁屏失败 |
