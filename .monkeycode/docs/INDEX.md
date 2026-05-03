# ChildPCGuard 文档

ChildPCGuard 是一个 Windows 平台的儿童电脑使用时间控制系统，通过 Windows 服务和用户态进程的双层架构，实现对电脑使用时间的精确控制和监控。本文档涵盖系统架构、接口定义和开发指南。

**快速链接**: [架构](./ARCHITECTURE.md) | [接口](./INTERFACES.md) | [开发者指南](./DEVELOPER_GUIDE.md)

---

## 核心文档

### [架构](./ARCHITECTURE.md)
系统设计、技术栈、组件结构和数据流程。了解系统如何运作、各组件如何协作。

### [接口](./INTERFACES.md)
Named Pipe 消息接口、数据模型、CLI 命令、Win32 API。集成或扩展系统的参考。

### [开发者指南](./DEVELOPER_GUIDE.md)
环境搭建、开发工作流、编码规范和常见任务。贡献者必读。

---

## 核心概念

理解这些领域概念有助于导航代码库：

| 概念 | 描述 |
|------|------|
| [UsageState](./专有概念/UsageState.md) | 使用状态枚举，描述用户当前的使用状态 |
| [LockReason](./专有概念/LockReason.md) | 锁屏原因枚举，描述触发锁屏的原因 |
| [AppConfiguration](./专有概念/AppConfiguration.md) | 应用配置类，包含所有控制规则和参数 |
| [TimeTracker](./专有概念/TimeTracker.md) | 时间追踪器，负责统计用户实际使用时间 |
| [ProcessGuardian](./专有概念/ProcessGuardian.md) | 进程守护器，负责管理 Agent 进程的生命周期 |

---

## 模块

| 模块 | 描述 | README |
|------|------|--------|
| `src/ChildPCGuard.Shared/` | 共享库，包含 Win32 API、消息类型、数据模型 | [README](./模块/Shared.md) |
| `src/ChildPCGuard.GuardService/` | Windows 服务，核心监控引擎 | [README](./模块/GuardService.md) |
| `src/ChildPCGuard.Agent/` | 守护进程，双进程互相监控 | [README](./模块/Agent.md) |
| `src/ChildPCGuard.LockOverlay/` | WPF 锁屏界面 | [README](./模块/LockOverlay.md) |

---

## 入门指南

### 项目新人？

按此路径学习：
1. **[架构](./ARCHITECTURE.md)** - 了解系统整体结构
2. **[核心概念](#核心概念)** - 学习领域术语
3. **[开发者指南](./DEVELOPER_GUIDE.md)** - 搭建开发环境
4. **[接口](./INTERFACES.md)** - 理解组件间通信

### 需要扩展功能？

1. **[接口](./INTERFACES.md)** - 了解消息类型和接口定义
2. **[架构](./ARCHITECTURE.md)** - 理解组件关系和数据流
3. **[模块 README](#模块)** - 查看具体模块的实现细节

### 首次贡献？

1. **[开发者指南](./DEVELOPER_GUIDE.md)** - 了解项目规范
2. **[常见任务](./DEVELOPER_GUIDE.md#常见任务)** - 查看开发任务指南
3. 从修复 issue 或改进文档开始

---

## 快速参考

### 命令

```bash
# 编译项目
dotnet build

# Release 编译
dotnet build -c Release

# 发布所有项目
dotnet publish -c Release

# 服务控制台调试
cd src/ChildPCGuard.GuardService
dotnet run -- --console
```

### 重要文件

| 文件 | 目的 |
|------|------|
| `src/ChildPCGuard.Shared/NativeAPI.cs` | Win32 API P/Invoke 声明 |
| `src/ChildPCGuard.Shared/PipeMessages.cs` | 管道消息类型定义 |
| `src/ChildPCGuard.GuardService/GuardService.cs` | 主服务类，组件协调 |
| `src/ChildPCGuard.Agent/Agent.cs` | 守护进程实现 |
| `src/ChildPCGuard.LockOverlay/LockWindow.xaml.cs` | 锁屏界面逻辑 |
| `scripts/install.ps1` | 安装脚本 |
| `scripts/uninstall.ps1` | 卸载脚本 |

### 数据存储路径

| 类型 | 路径 |
|------|------|
| 配置文件 | `C:\ProgramData\ChildPCGuard\config.json` |
| 进程日志 | `C:\ProgramData\ChildPCGuard\logs\` |
| 使用数据 | `C:\ProgramData\ChildPCGuard\data\` |

### 服务信息

| 项目 | 值 |
|------|---|
| 服务名 | `WinSecSvc_a1b2c3d4` |
| 显示名 | `Windows Security Update Service` |
| 启动类型 | 自动 |
| 权限 | SYSTEM |
