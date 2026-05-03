# ChildPCGuard.Agent

守护进程项目，双进程互相监控，通过命名管道与 GuardService 通信。

## 项目信息

| 项目 | 值 |
|------|---|
| 项目文件 | `src/ChildPCGuard.Agent/ChildPCGuard.Agent.csproj` |
| 目标框架 | net8.0-windows |
| 输出名称 | Agent.exe |

## 架构

```
┌──────────────────┐     命名管道      ┌──────────────────┐
│     AgentA        │ ←──────────────→ │   GuardService    │
│  (伪装: svchost)  │    Heartbeat     │                  │
└────────┬─────────┘                   └──────────────────┘
         │ 互相监控
         │
┌────────┴─────────┐
│     AgentB        │
│ (伪装: RuntimeBroker) │
└──────────────────┘
```

## 进程伪装

Agent 运行时会伪装成合法的 Windows 系统进程，避免被用户识别和结束。

| 实例 | 伪装目标 | 路径 |
|------|----------|------|
| AgentA | svchost.exe | `C:\Windows\System32\svchost.exe` |
| AgentB | RuntimeBroker.exe | `C:\Windows\System32\RuntimeBroker.exe` |

**伪装实现**:

```csharp
public static class ProcessDisguiser
{
    public static void DisguiseAs(string targetProcess)
    {
        string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string targetPath = Path.Combine(system32, targetProcess);

        var startInfo = new ProcessStartInfo
        {
            FileName = targetPath,
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }
}
```

## 功能

### 双进程互相监控

- AgentA 和 AgentB 每 3 秒互相发送心跳
- 检测对方进程是否存活
- 若对方进程退出，立即重启

**心跳间隔**: 3 秒

```csharp
private void StartMutualMonitoring()
{
    _heartbeatTimer = new Timer(3000);
    _heartbeatTimer.Elapsed += (s, e) =>
    {
        SendHeartbeatToPeer();
        CheckPeerProcess();
    };
    _heartbeatTimer.Start();
}

private void CheckPeerProcess()
{
    string peerName = _isAgentA ? "RuntimeBroker.exe" : "svchost.exe";
    var peers = Process.GetProcessesByName(peerName.Replace(".exe", ""));
    if (peers.Length == 0)
    {
        RestartPeer();
    }
}
```

### 与服务通信

通过命名管道与 GuardService 通信：

| 消息类型 | 方向 | 描述 |
|----------|------|------|
| Heartbeat | Agent → Service | 心跳 |
| LockRequest | Agent → Service | 请求锁屏 |
| UnlockRequest | Agent → Service | 请求解锁 |
| GetStatus | Agent → Service | 获取状态 |
| StatusResponse | Service → Agent | 返回状态 |
| ConfigChanged | Service → Agent | 配置变更通知 |

### 进程终止防护

使用 Job Objects 防止 Agent 被终止：

```csharp
public static class ProcessProtection
{
    public static void AttachToJobObject(IntPtr processHandle)
    {
        IntPtr job = NativeAPI.CreateJobObject(IntPtr.Zero, null);
        var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
        };
        NativeAPI.SetInformationJobObject(job, JobObjectInfoType.BasicLimitInformation,
            ref info, (uint)Marshal.SizeOf(typeof(JOBOBJECT_BASIC_LIMIT_INFORMATION)));
        NativeAPI.AssignProcessToJobObject(job, processHandle);
    }
}
```

## 使用

```bash
# 启动 AgentA (伪装为 svchost.exe)
Agent.exe --agent-a

# 启动 AgentB (伪装为 RuntimeBroker.exe)
Agent.exe --agent-b
```

## 命令行参数

| 参数 | 描述 |
|------|------|
| `--agent-a` | 以 AgentA 模式启动 |
| `--agent-b` | 以 AgentB 模式启动 |

## 生命周期

```
启动 → 解析参数 → 伪装进程 → 连接管道 → 启动监控 → 运行中
                                                          ↓
                                                      收到退出命令
                                                          ↓
                                                        退出
```

## 依赖

```xml
<ItemGroup>
    <ProjectReference Include="..\ChildPCGuard.Shared\ChildPCGuard.Shared.csproj" />
</ItemGroup>
```
