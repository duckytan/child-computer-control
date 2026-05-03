# ChildPCGuard.GuardService

Windows 服务项目，核心监控引擎。以 SYSTEM 权限运行，实现所有监控逻辑。

## 项目信息

| 项目 | 值 |
|------|---|
| 项目文件 | `src/ChildPCGuard.GuardService/ChildPCGuard.GuardService.csproj` |
| 目标框架 | net8.0-windows |
| 服务名 | `WinSecSvc_a1b2c3d4` |
| 显示名 | `Windows Security Update Service` |
| 启动类型 | 自动 |

## 架构

```
┌─────────────────────────────────────────────────────────────┐
│                      GuardService                            │
│                    (Windows Service)                         │
├─────────────────────────────────────────────────────────────┤
│  GuardService (主类)                                         │
│  ├── ConfigManager        配置加载/保存                       │
│  ├── NamedPipeServer     管道服务器 (接收Agent消息)            │
│  ├── TimeTracker         时间追踪                           │
│  ├── ProcessGuardian     进程守护                           │
│  ├── AppMonitor          程序监控                           │
│  ├── WebMonitor          网站监控                           │
│  ├── ShutdownScheduler   关机调度                           │
│  └── NtpValidator        NTP验证                            │
└─────────────────────────────────────────────────────────────┘
```

## 核心组件

### GuardService

主服务类，协调所有组件。

**文件**: `GuardService.cs`

**职责**:
- 服务生命周期管理 (OnStart, OnStop)
- 定时监控循环 (MonitoringCallback)
- 锁屏/关机触发 (TriggerLockScreen, TriggerShutdown)
- Named Pipe 消息处理

**关键字段**:

```csharp
private ServiceHost _serviceHost;
private ConfigManager _configManager;
private NamedPipeServer _pipeServer;
private TimeTracker _timeTracker;
private ProcessGuardian _processGuardian;
private AppMonitor _appMonitor;
private WebMonitor _webMonitor;
private ShutdownScheduler _shutdownScheduler;
private NtpValidator _ntpValidator;
private Timer _monitoringTimer;
```

**监控循环**:

```csharp
private void MonitoringCallback(object state)
{
    // 1. 检查时间篡改
    if (!_ntpValidator.ValidateTime())
    {
        TriggerLockScreen(LockReason.TimeTampered);
        return;
    }

    // 2. 检查使用状态
    var stateInfo = _timeTracker.GetState();
    switch (stateInfo.State)
    {
        case UsageState.Using:
            if (stateInfo.ContinuousTime >= TimeSpan.FromMinutes(_config.Config.ContinuousLimitMinutes))
            {
                _timeTracker.StartRest();
                TriggerLockScreen(LockReason.ContinuousLimit);
            }
            break;
        case UsageState.Resting:
            if (stateInfo.RestRemainingTime <= TimeSpan.Zero)
                _timeTracker.EndRest();
            break;
        // ...
    }

    // 3. 检查黑名单
    if (_appMonitor.IsBlockedProcessRunning())
        TriggerLockScreen(LockReason.BlockedApp);

    if (_webMonitor.IsBlockedSiteAccessed())
        TriggerLockScreen(LockReason.BlockedSite);

    // 4. 检查定时关机
    if (_shutdownScheduler.ShouldShutdown())
        TriggerShutdown();
}
```

### ConfigManager

配置管理。

**文件**: `ConfigManager.cs`

**职责**:
- 从 JSON 文件加载配置
- 保存配置到 JSON 文件
- 监控配置文件变更

**数据路径**: `C:\ProgramData\ChildPCGuard\config.json`

**关键方法**:

```csharp
public AppConfiguration Load()
{
    string path = Path.Combine(DataDirectory, "config.json");
    string json = File.ReadAllText(path);
    return JsonSerializer.Deserialize<AppConfiguration>(json);
}

public void Save(AppConfiguration config)
{
    string path = Path.Combine(DataDirectory, "config.json");
    string json = JsonSerializer.Serialize(config);
    File.WriteAllText(path, json);
}
```

### NamedPipeServer

命名管道服务器。

**文件**: `NamedPipeServer.cs`

**管道名称**: `ChildPCGuardPipe`

**处理的消息类型**:

| 类型 | 处理逻辑 |
|------|----------|
| Heartbeat | 更新 Agent 状态，重置超时计数器 |
| LockRequest | 触发锁屏 |
| UnlockRequest | 验证密码，解除锁屏 |
| RestartAgent | 重启指定的 Agent 进程 |
| GetStatus | 返回当前状态 |
| LogRecord | 记录日志 |
| AddTime | 增加使用时间 |
| PauseControl | 暂停/恢复控制 |

**使用 BinaryFormatter 序列化**:

```csharp
public void HandleClient(PipeClientHandler handler)
{
    var message = (PipeMessage)formatter.Deserialize(handler.Reader);
    switch (message.Type)
    {
        case PipeMessageType.Heartbeat:
            HandleHeartbeat((HeartbeatMessage)message);
            break;
        // ...
    }
}
```

### TimeTracker

时间追踪器。

**文件**: `TimeTracker.cs`

**职责**:
- 跟踪总使用时间
- 跟踪连续使用时间
- 管理休息倒计时
- 判断是否在允许时间窗口内

**状态**: `UsageState` (Using, Resting, Locked, Shutdown, Paused, Normal)

**关键方法**:

```csharp
public void RecordActivity(DateTime now)    // 记录用户活动
public void StartRest()                      // 开始强制休息
public void EndRest()                        // 结束强制休息
public (UsageState State, TimeSpan UsedTime, ...) GetState()  // 获取当前状态
```

### ProcessGuardian

进程守护器。

**文件**: `ProcessGuardian.cs`

**职责**:
- 启动 AgentA 和 AgentB
- 监控 Agent 心跳超时
- 超时时重启 Agent

**心跳超时阈值**: 15 秒

```csharp
public void CheckAgents()
{
    foreach (var agent in _agents.Values)
    {
        if (DateTime.Now - agent.LastHeartbeat > TimeSpan.FromSeconds(15))
        {
            RestartAgent(agent.Name);
        }
    }
}
```

### AppMonitor

程序监控器。

**文件**: `AppMonitor.cs`

**职责**:
- 枚举运行中的进程
- 检测黑名单程序
- 记录黑名单程序启动时间

**黑名单来源**: `AppConfiguration.BlockedApps`

### WebMonitor

网站监控器。

**文件**: `WebMonitor.cs`

**职责**:
- 监控浏览器进程
- 检查历史记录中的黑名单网站
- 支持 Chrome, Firefox, Edge

**黑名单来源**: `AppConfiguration.BlockedSites`

**检测方式**: 读取浏览器历史记录数据库

### ShutdownScheduler

关机调度器。

**文件**: `ShutdownScheduler.cs`

**职责**:
- 检查是否到达关机时间
- 触发系统关机

**关机时间来源**: `AppConfiguration.AutoShutdownTime`

```csharp
public bool ShouldShutdown()
{
    var now = DateTime.Now;
    var shutdownTime = DateTime.Today + TimeSpan.Parse(_config.AutoShutdownTime);
    return now >= shutdownTime;
}
```

### NtpValidator

NTP 时间验证器。

**文件**: `NtpValidator.cs`

**职责**:
- 从 NTP 服务器获取标准时间
- 与本地时间比较
- 检测时间篡改

**NTP 服务器**: `pool.ntp.org`, `time.windows.com`

**容差**: 5 分钟 (可配置)

```csharp
public bool ValidateTime()
{
    var ntpTime = GetNtpTime();
    var localTime = DateTime.Now;
    var diff = Math.Abs((ntpTime - localTime).TotalMinutes);
    return diff <= _config.NtpToleranceMinutes;
}
```

## 服务安装

```powershell
# 安装服务
New-Service -Name "WinSecSvc_a1b2c3d4" `
    -BinaryPathName "C:\Program Files\ChildPCGuard\ChildPCGuard.GuardService.exe" `
    -DisplayName "Windows Security Update Service" `
    -StartType Automatic `
    -Description "Provides Windows security update monitoring services" `
    -ErrorAction SilentlyContinue

# 启动服务
Start-Service -Name "WinSecSvc_a1b2c3d4"
```

## 调试

**控制台模式运行**:

```powershell
cd src/ChildPCGuard.GuardService
dotnet run -- --console
```

## 依赖

```xml
<ItemGroup>
    <ProjectReference Include="..\ChildPCGuard.Shared\ChildPCGuard.Shared.csproj" />
</ItemGroup>
```
