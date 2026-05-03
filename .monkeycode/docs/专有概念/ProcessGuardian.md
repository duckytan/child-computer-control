# ProcessGuardian

ProcessGuardian 是进程守护组件，负责管理 Agent 进程的生命周期。

## 什么是 ProcessGuardian？

ProcessGuardian 是 GuardService 的组件，负责启动和监控 Agent 进程（AgentA 和 AgentB）。它通过心跳机制检测 Agent 是否存活，并在超时时重启它们。

**关键特征**:
- 管理 AgentA 和 AgentB 两个进程
- 每秒检测一次心跳
- 15 秒超时重启
- 防止 Agent 被意外终止

## 代码位置

| 方面 | 位置 |
|------|------|
| 实现 | `src/ChildPCGuard.GuardService/ProcessGuardian.cs` |
| 使用 | `src/ChildPCGuard.GuardService/GuardService.cs` |

## 架构

```
┌─────────────────────────────────────────────────────┐
│                  ProcessGuardian                     │
├─────────────────────────────────────────────────────┤
│  _agents: Dictionary<string, AgentInfo>            │
│                                                     │
│  AgentA ──── 3秒心跳 ────→ GuardService            │
│  AgentB ──── 3秒心跳 ────→ GuardService            │
│                                                     │
│  心跳超时(15秒) ──→ 重启 Agent                     │
└─────────────────────────────────────────────────────┘
```

## 核心功能

### 启动 Agent

```csharp
public void StartAgents()
{
    // 启动 AgentA (伪装为 svchost.exe)
    StartAgent("AgentA", "svchost.exe");

    // 启动 AgentB (伪装为 RuntimeBroker.exe)
    StartAgent("AgentB", "RuntimeBroker.exe");
}

private void StartAgent(string name, string disguise)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "Agent.exe",
            Arguments = name == "AgentA" ? "--agent-a" : "--agent-b",
            UseShellExecute = true
        }
    };
    process.Start();

    _agents[name] = new AgentInfo
    {
        Name = name,
        Process = process,
        DisguiseAs = disguise,
        LastHeartbeat = DateTime.Now
    };
}
```

### 心跳检测

```csharp
public void CheckAgents()
{
    foreach (var kvp in _agents)
    {
        var agent = kvp.Value;
        var elapsed = DateTime.Now - agent.LastHeartbeat;

        if (elapsed > TimeSpan.FromSeconds(15))
        {
            EventLog.WriteEntry($"Agent {agent.Name} heartbeat timeout, restarting...",
                EventLogEntryType.Warning);
            RestartAgent(agent.Name);
        }
    }
}
```

### 重启 Agent

```csharp
public void RestartAgent(string name)
{
    var agent = _agents[name];
    if (agent.Process != null && !agent.Process.HasExited)
    {
        agent.Process.Kill();
        agent.Process.WaitForExit();
    }

    // 重新启动
    StartAgent(name, agent.DisguiseAs);
}
```

## AgentInfo 结构

```csharp
private class AgentInfo
{
    public string Name { get; set; }              // "AgentA" 或 "AgentB"
    public Process Process { get; set; }          // 进程对象
    public string DisguiseAs { get; set; }       // 伪装目标进程名
    public DateTime LastHeartbeat { get; set; }  // 最后心跳时间
}
```

## 与 Agent 的通信

Agent 通过命名管道发送心跳：

```csharp
// Agent 端
var heartbeat = new HeartbeatMessage
{
    Type = PipeMessageType.Heartbeat,
    ProcessName = "svchost.exe",  // 或 "RuntimeBroker.exe"
    ProcessId = Process.GetCurrentProcess().Id,
    Timestamp = DateTime.Now
};

// 发送心跳到 Service
_pipeClient.Write(heartbeat);
```

```csharp
// Service 端 (NamedPipeServer)
case PipeMessageType.Heartbeat:
    var hb = (HeartbeatMessage)message;
    _processGuardian.UpdateHeartbeat(hb.ProcessName);
    break;
```

## 三层守护机制

```
Layer 1: AgentA ↔ AgentB 互相监控 (3秒心跳)
Layer 2: GuardService 接收心跳，超时重启 (15秒超时)
Layer 3: Windows 服务自动重启 (sc failure)
```

| 层级 | 监控频率 | 超时时间 | 动作 |
|------|----------|----------|------|
| 1 | 3 秒 | 9 秒 | Agent 互相重启 |
| 2 | 1 秒 | 15 秒 | Service 重启 Agent |
| 3 | - | - | sc 失败自动重启服务 |

## 相关概念

- [Agent](./模块/Agent.md) - Agent 模块
- [GuardService](./模块/GuardService.md) - ProcessGuardian 所属的服务
