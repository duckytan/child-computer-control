# 开发者指南

## 项目目的

ChildPCGuard 是一个 Windows 平台的儿童电脑使用时间控制系统，帮助家长控制孩子使用电脑的时间。

**核心职责**:
- 限制每日电脑使用总时长
- 强制休息模式（连续使用后休息）
- 定时自动关机
- 程序和网站使用监控
- 黑名单功能
- 进程保护（防止被终止）

**相关系统**:
- Windows Service (系统级权限运行)
- NTP 服务器 (时间验证)

## 环境搭建

### 前置条件

- Windows 10/11 专业版
- .NET 8 SDK
- Visual Studio 2022 或 VS Code (可选)

### 安装

```bash
# 克隆仓库
git clone https://github.com/duckytan/child-computer-control.git
cd child-computer-control

# 进入项目目录
cd ChildPCGuard

# 还原依赖
dotnet restore

# 编译项目
dotnet build
```

### 项目结构

```
ChildPCGuard/
├── ChildPCGuard.sln              # 解决方案文件
├── src/
│   ├── ChildPCGuard.Shared/     # 共享库项目
│   ├── ChildPCGuard.GuardService/  # Windows 服务项目
│   ├── ChildPCGuard.Agent/     # 守护进程项目
│   └── ChildPCGuard.LockOverlay/   # WPF 锁屏界面项目
├── scripts/
│   ├── install.ps1              # 安装脚本
│   └── uninstall.ps1            # 卸载脚本
└── docs/                        # 工程文档
```

## 开发工作流

### 编译

```bash
# Debug 编译
dotnet build

# Release 编译
dotnet build -c Release

# 发布所有项目
dotnet publish -c Release
```

### 运行测试

> 当前版本尚未实现自动化测试

```bash
dotnet test
```

### 代码质量

- 使用 .NET 默认代码分析规则
- 遵循 C# 命名规范
- 关键类和方法需要 XML 文档注释

### 调试

**服务调试 (控制台模式)**:
```powershell
cd src/ChildPCGuard.GuardService
dotnet run -- --console
```

**附加调试器**:
1. 在 Visual Studio 中打开解决方案
2. 设置断点
3. 附加到进程 (svchost.exe 或 RuntimeBroker.exe)

## 常见任务

### 添加新的管道消息类型

**需修改的文件**:
1. `src/ChildPCGuard.Shared/PipeMessages.cs` - 添加消息类型和类
2. `src/ChildPCGuard.GuardService/NamedPipeServer.cs` - 处理新消息类型

**步骤**:
1. 在 `PipeMessageType` 枚举中添加新类型
2. 创建新的消息类 (如需要)
3. 在 `NamedPipeServer.HandleClient()` 中添加消息处理逻辑

### 添加新的监控功能

**需修改的文件**:
1. `src/ChildPCGuard.GuardService/GuardService.cs` - 添加新组件引用
2. 创建新的监控类

**步骤**:
1. 在 `GuardService` 类中添加新组件字段
2. 在 `Start()` 方法中初始化组件
3. 在 `MonitoringCallback()` 中调用组件逻辑

### 修改锁屏界面

**需修改的文件**:
1. `src/ChildPCGuard.LockOverlay/LockWindow.xaml` - 修改 UI
2. `src/ChildPCGuard.LockOverlay/LockWindow.xaml.cs` - 修改逻辑

### 修改配置结构

**需修改的文件**:
1. `src/ChildPCGuard.Shared/Models.cs` - 修改 `AppConfiguration` 类
2. `src/ChildPCGuard.GuardService/ConfigManager.cs` - 修改序列化逻辑

## 编码规范

### 文件组织

- 每个类一个文件
- 文件名与类名一致
- 相关文件放在同一目录

### 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 类/接口 | PascalCase | `TimeTracker`, `IConfigManager` |
| 方法 | PascalCase | `GetState()`, `StartRest()` |
| 字段/属性 | PascalCase | `_configManager`, `ServiceName` |
| 常量 | PascalCase | `DataDirectory`, `DefaultIV` |
| 枚举值 | PascalCase | `UsageState.Using` |

### 命名空间

```
ChildPCGuard.Shared          # 共享库
ChildPCGuard.GuardService    # Windows 服务
ChildPCGuard.Agent          # 守护进程
ChildPCGuard.LockOverlay     # 锁屏界面
```

### 错误处理

```csharp
// 推荐：记录错误并继续
try
{
    // 操作
}
catch (Exception ex)
{
    EventLog.WriteEntry($"操作失败: {ex.Message}", EventLogEntryType.Warning);
}

// 避免：吞掉异常不做处理
try
{
    // 操作
}
catch { }
```

### 日志记录

使用 Windows 事件日志记录：

```csharp
EventLog.WriteEntry("消息内容", EventLogEntryType.Information);   // 信息
EventLog.WriteEntry("警告内容", EventLogEntryType.Warning);       // 警告
EventLog.WriteEntry("错误内容", EventLogEntryType.Error);         // 错误
```

## 项目特定规范

### 服务配置

- 服务名: `WinSecSvc_a1b2c3d4` (不可更改)
- 显示名: `Windows Security Update Service`
- 服务无法被停止 (`CanStop = false`)

### 进程伪装

| 进程 | 伪装目标 | 路径 |
|------|----------|------|
| AgentA | svchost.exe | `C:\Windows\System32\svchost.exe` |
| AgentB | RuntimeBroker.exe | `C:\Windows\System32\RuntimeBroker.exe` |

### 数据存储

所有数据存储在 `C:\ProgramData\ChildPCGuard\` 目录下，包括：
- `config.json` - 主配置
- `logs/` - 日志文件
- `data/` - 使用数据

### 三层守护

1. AgentA ↔ AgentB 互相监控 (3秒心跳)
2. GuardService 接收心跳，超时重启
3. Windows 服务自动重启配置

## 安全考虑

- 配置文件加密存储
- 密码使用 AES 加密
- NTP 验证防止时间篡改
- 检测安全模式启动自动关机
