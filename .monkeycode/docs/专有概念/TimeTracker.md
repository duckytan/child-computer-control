# TimeTracker

TimeTracker 是时间追踪组件，负责统计用户实际使用电脑的时间。

## 什么是 TimeTracker？

TimeTracker 是 GuardService 的核心组件之一，负责跟踪用户的实际使用时间。它通过检测用户输入活动（键盘、鼠标）来判断用户是否正在使用电脑，并累计使用时长。

**关键特征**:
- 基于最后输入时间判断用户活动
- 区分工作日和周末规则
- 支持强制休息模式
- 支持时间窗口限制

## 代码位置

| 方面 | 位置 |
|------|------|
| 实现 | `src/ChildPCGuard.GuardService/TimeTracker.cs` |
| 配置 | `src/ChildPCGuard.GuardService/ConfigManager.cs` |
| 状态定义 | `src/ChildPCGuard.Shared/Models.cs` |

## 核心概念

### 空闲检测

使用 Win32 API `GetLastInputInfo` 获取最后输入时间：

```csharp
public bool IsUserActive()
{
    var lastInput = new LASTINPUTINFO();
    lastInput.cbSize = (uint)Marshal.SizeOf(lastInput);
    NativeAPI.GetLastInputInfo(ref lastInput);

    uint idleMs = (uint)Environment.TickCount - lastInput.dwTime;
    return idleMs < _config.IdleThresholdMs;  // 默认 5000ms
}
```

### 使用时间累计

```csharp
public void RecordActivity(DateTime now)
{
    if (IsUserActive())
    {
        _lastActivityTime = now;

        if (_state != UsageState.Using)
        {
            _state = UsageState.Using;
            OnStateChanged?.Invoke(UsageState.Using);
        }

        // 累计使用时间
        _todayData.TotalUsedTime += TimeSpan.FromSeconds(_checkIntervalSeconds);
        _todayData.ContinuousUsedTime += TimeSpan.FromSeconds(_checkIntervalSeconds);
    }
}
```

### 时间窗口检查

检查当前时间是否在允许的时间窗口内：

```csharp
public bool IsWithinAllowedTimeWindow(DateTime now)
{
    var rule = IsWeekend(now) ? _config.Rules.Weekends : _config.Rules.Weekdays;

    foreach (var window in rule.AllowedTimeWindows)
    {
        var start = TimeSpan.Parse(window.Start);
        var end = TimeSpan.Parse(window.End);
        var current = now.TimeOfDay;

        if (current >= start && current <= end)
            return true;
    }
    return false;
}
```

## 状态管理

### UsageState 转换

```csharp
public void StartRest()
{
    _state = UsageState.Resting;
    _restEndTime = DateTime.Now + TimeSpan.FromMinutes(_config.RestDurationMinutes);
    OnStateChanged?.Invoke(UsageState.Resting);
}

public void EndRest()
{
    _state = UsageState.Using;
    _todayData.ContinuousUsedTime = TimeSpan.Zero;
    OnStateChanged?.Invoke(UsageState.Using);
}

public void Pause()
{
    _previousState = _state;
    _state = UsageState.Paused;
    OnStateChanged?.Invoke(UsageState.Paused);
}
```

## 统计数据

### DailyUsageData

```csharp
public class DailyUsageData
{
    public DateTime Date { get; set; }
    public TimeSpan TotalUsedTime { get; set; }      // 当日总使用时间
    public TimeSpan ContinuousUsedTime { get; set; } // 连续使用时间
    public int ExtraMinutesToday { get; set; }       // 额外增加的分钟数
    public UsageState CurrentState { get; set; }     // 当前状态
}
```

## 相关概念

- [UsageState](./UsageState.md) - 使用状态
- [AppConfiguration](./AppConfiguration.md) - 配置中的时间规则
- [GuardService](./模块/GuardService.md) - TimeTracker 所属的服务
