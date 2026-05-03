# 需求实施计划

## 项目概述

- **项目名称**: ChildPCGuard - 儿童电脑使用时间控制程序
- **技术栈**: C# .NET 8, Windows Service, WPF, Win32 API
- **架构**: Windows服务(SYSTEM权限) + 双守护进程(用户态) + 锁屏界面

## 项目结构

```
ChildPCGuard/
├── ChildPCGuard.sln
├── src/
│   ├── ChildPCGuard.Shared/        # 共享库
│   ├── ChildPCGuard.GuardService/   # Windows服务
│   ├── ChildPCGuard.Agent/         # 守护进程
│   └── ChildPCGuard.LockOverlay/   # 锁屏界面
├── scripts/
│   ├── install.ps1
│   └── uninstall.ps1
├── docs/
│   ├── 01-需求规格说明书.md
│   ├── 02-技术设计说明书.md
│   ├── 03-测试方案.md
│   ├── 04-验收方案.md
│   ├── 05-用户手册.md
│   └── 06-部署方案.md
└── tests/
```

---

## 阶段一：工程文档准备

- [x] 1. 编写需求规格说明书 (docs/01-需求规格说明书.md)
   - [x] 1.1 项目背景和目标
   - [x] 1.2 功能需求详细描述
   - [x] 1.3 非功能需求（性能、安全、可靠性）
   - [x] 1.4 用例图和用户故事
   - [x] 1.5 需求追踪矩阵

- [x] 2. 编写技术设计说明书 (docs/02-技术设计说明书.md)
   - [x] 2.1 系统架构设计
   - [x] 2.2 模块设计和接口定义
   - [x] 2.3 数据库/配置文件设计
   - [x] 2.4 进程间通信设计
   - [x] 2.5 安全设计方案

- [x] 3. 编写测试方案 (docs/03-测试方案.md)
   - [x] 3.1 测试策略和测试类型
   - [x] 3.2 单元测试方案
   - [x] 3.3 集成测试方案
   - [x] 3.4 系统测试方案
   - [x] 3.5 验收测试方案
   - [x] 3.6 测试用例设计

- [x] 4. 编写验收方案 (docs/04-验收方案.md)
   - [x] 4.1 验收标准和验收准则
   - [x] 4.2 验收测试项
   - [x] 4.3 验收流程
   - [x] 4.4 验收签字确认

- [x] 5. 编写用户手册 (docs/05-用户手册.md)
   - [x] 5.1 产品介绍
   - [x] 5.2 安装指南
   - [x] 5.3 使用指南
   - [x] 5.4 配置说明
   - [x] 5.5 常见问题解答

- [x] 6. 编写部署方案 (docs/06-部署方案.md)
   - [x] 6.1 部署架构
   - [x] 6.2 部署流程
   - [x] 6.3 回滚方案
   - [x] 6.4 应急响应

---

## 阶段二：代码开发

- [x] 7. 创建解决方案和项目结构
   - 创建 ChildPCGuard.sln
   - 创建 src/ 目录下的四个项目
   - 配置项目引用关系：Shared 被 Service/Agent/LockOverlay 引用

- [x] 8. 实现共享库 (ChildPCGuard.Shared)
   - [x] 8.1 实现 NativeAPI.cs
     - 声明 User32.dll 和 Kernel32.dll 的 P/Invoke 方法
     - 定义 LASTINPUTINFO, STARTUPINFO, PROCESS_INFORMATION 等结构体
     - 定义桌面操作、进程操作、系统关闭等相关常量

   - [x] 8.2 实现 PipeMessages.cs
     - 定义 PipeMessageType 枚举（Heartbeat, LockRequest, UnlockRequest等）
     - 实现 PipeMessage, HeartbeatMessage, StatusMessage 消息类

   - [x] 8.3 实现 Models.cs
     - 实现 AppConfiguration 配置类（含RulesConfiguration, TimeRule, TimeWindow）
     - 实现 DailyUsageData, ProcessUsageLog, WebUsageLog 等数据类
     - 定义 UsageState, LockReason 枚举

   - [x] 8.4 实现 AesEncryption.cs
     - 实现 Encrypt/Decrypt 对称加密方法
     - 实现 GenerateKey 密钥生成

   - [x] 8.5 为共享库编写单元测试
     - 为 AesEncryption 加密/解密编写测试
     - 为 PipeMessage 序列化/反序列化编写测试
     - 为 Models 数据模型验证编写测试

- [x] 9. 实现 Windows服务 (ChildPCGuard.GuardService)
   - [x] 9.1 实现 ConfigManager.cs
     - 从 C:\ProgramData\ChildPCGuard\config.json 加载/保存配置
     - 实现配置验证和加密存储

   - [x] 9.2 实现 NamedPipeServer.cs
     - 实现命名管道服务器，接受 Agent 的连接
     - 处理 PipeMessage 并触发相应回调

   - [x] 9.3 实现 TimeTracker.cs
     - 使用 GetLastInputInfo Win32 API 跟踪用户实际使用时间
     - 实现 GetState() 返回当前 UsageState 和剩余时间
     - 实现 StartRest/EndRest/ResetDaily 方法
     - 支持工作日/周末不同规则

   - [x] 9.3.1 为 TimeTracker 编写单元测试
     - 测试 GetState 返回正确的 UsageState
     - 测试 IsDailyLimitReached 边界条件
     - 测试工作日/周末规则切换

   - [x] 9.4 实现 ProcessGuardian.cs
     - 启动并管理 AgentA (svchost伪装) 和 AgentB (RuntimeBroker伪装)
     - 监控心跳超时，超时则重启对应 Agent
     - 实现 RestartAgent 方法

   - [x] 9.5 实现 AppMonitor.cs
     - 获取前台窗口进程信息
     - 检查黑名单程序是否运行
     - 记录进程使用日志到 logs/YYYY-MM-DD_process.json

   - [x] 9.6 实现 WebMonitor.cs
     - 读取 Chrome/Edge/Firefox 浏览器历史记录
     - 检查黑名单网站访问
     - 记录网站访问日志到 logs/YYYY-MM-DD_web.json

   - [x] 9.7 实现 ShutdownScheduler.cs
     - 检查是否到达定时关机时间
     - 在关机前显示60秒警告通知

   - [x] 9.8 实现 NtpValidator.cs
     - 通过 NTP 服务器验证系统时间
     - 检测时间篡改行为

   - [x] 9.9 实现 NotificationHelper.cs
     - 显示警告通知（剩余时间提醒）

   - [x] 9.10 实现 GuardService.cs (主服务)
     - 实现 ServiceBase 派生类的 OnStart/OnStop/OnShutdown
     - 初始化所有组件并启动定时器
     - 实现 TriggerLockScreen 和 TriggerShutdown 方法
     - 处理管道消息分发

   - [x] 9.11 实现 Program.cs
     - 支持 --console 参数以控制台模式运行（用于调试）
     - 支持 --install/--uninstall 服务安装/卸载

   - [x] 9.12 为 GuardService 编写集成测试
     - 测试服务启动和停止流程
     - 测试 TriggerLockScreen 调用
     - 测试配置变更后服务响应

- [x] 10. 实现守护进程 (ChildPCGuard.Agent)
   - [x] 10.1 实现 Agent.cs
     - 实现与配对进程的互相监控
     - 通过命名管道向 Service 发送心跳
     - 检测到配对进程死亡时通知 Service 重启

   - [x] 10.2 实现 Program.cs
     - 支持 --agent-a 和 --agent-b 参数启动不同角色
     - 无窗口、无托盘，静默运行

   - [x] 10.3 为 Agent 编写单元测试
     - 测试心跳发送逻辑
     - 测试配对进程死亡检测
     - 测试命令行参数解析

- [x] 11. 实现锁屏界面 (ChildPCGuard.LockOverlay)
   - [x] 11.1 实现 LockWindow.xaml
     - 全屏显示锁屏界面
     - 显示剩余时间/锁屏原因
     - 密码输入框（密码错误显示提示）
     - 紧急解锁快捷键提示（Ctrl+Alt+Shift+F12）

   - [x] 11.2 实现 LockWindow.xaml.cs
     - 使用 CreateDesktop/SwitchDesktop 创建自定义桌面
     - 验证解锁密码（通过命名管道询问 Service）
     - 5分钟内3次密码错误锁定5分钟
     - 实现紧急解锁功能

   - [x] 11.3 为 LockOverlay 编写单元测试
     - 测试密码验证逻辑
     - 测试错误计数锁定逻辑
     - 测试紧急解锁快捷键检测

- [x] 12. 创建项目文件和配置
   - [x] 12.1 创建 ChildPCGuard.Shared.csproj
     - 配置 .NET 8, 类库项目

   - [x] 12.2 创建 ChildPCGuard.GuardService.csproj
     - 配置 .NET 8, Windows Service 项目
     - 引用 Shared 项目

   - [x] 12.3 创建 ChildPCGuard.Agent.csproj
     - 配置 .NET 8, 控制台应用
     - 引用 Shared 项目

   - [x] 12.4 创建 ChildPCGuard.LockOverlay.csproj
     - 配置 .NET 8, WPF 应用
     - 引用 Shared 项目

   - [x] 12.5 创建 ChildPCGuard.sln
     - 添加所有项目引用

- [x] 13. 实现安装脚本
   - [x] 13.1 实现 install.ps1
     - 检查管理员权限
     - 创建 C:\ProgramData\ChildPCGuard 目录
     - 复制文件到 System32（使用伪装文件名）
     - 生成加密密钥
     - 注册 Windows 服务
     - 设置服务失败后自动重启

   - [x] 13.2 实现 uninstall.ps1
     - 验证管理员权限和密码
     - 停止并删除服务
     - 删除伪装文件和安装目录
     - 清理计划任务

---

## 阶段三：测试与验收

- [ ] 14. 检查点 - 确保项目编译通过
   - 确保 dotnet build 成功
   - 确保所有引用正确

- [ ] 15. 单元测试执行
   - [ ]* 15.1 执行共享库单元测试
   - [ ]* 15.2 执行 TimeTracker 单元测试
   - [ ]* 15.3 执行 Agent 单元测试
   - [ ]* 15.4 执行 LockOverlay 单元测试

- [ ] 16. 集成测试
   - [ ]* 16.1 服务与 Agent 集成测试
   - [ ]* 16.2 命名管道通信测试
   - [ ]* 16.3 配置管理集成测试

- [ ] 17. 系统测试
   - [ ]* 17.1 完整安装流程测试
   - [ ]* 17.2 时间限制功能测试
   - [ ]* 17.3 强制休息功能测试
   - [ ]* 17.4 定时关机功能测试
   - [ ]* 17.5 黑名单功能测试
   - [ ]* 17.6 进程守护功能测试
   - [ ]* 17.7 卸载流程测试

- [ ] 18. 验收测试
   - [ ]* 18.1 验收测试执行
   - [ ]* 18.2 验收报告编写
   - [ ]* 18.3 验收签字确认

---

## 技术要点

### 进程伪装
- AgentA.exe → C:\Windows\System32\svchost.exe
- AgentB.exe → C:\Windows\System32\RuntimeBroker.exe
- 服务名：WinSecSvc_a1b2c3d4，显示名：Windows Security Update Service

### 三层守护机制
1. AgentA ↔ AgentB 互相监控
2. Service 接收心跳，超时则重启 Agent
3. Service 配置自动重启

### 数据存储路径
- 配置: C:\ProgramData\ChildPCGuard\config.json
- 日志: C:\ProgramData\ChildPCGuard\logs\
- 数据: C:\ProgramData\ChildPCGuard\data\

### 关键 Win32 API
- GetLastInputInfo - 获取用户最后输入时间
- CreateDesktop/SwitchDesktop - 自定义锁屏桌面
- LockWorkStation - 锁定工作站
- shutdown.exe - 定时关机

---

## 依赖关系

```
Shared (类库)
    ↓
Service ←→ Agent ←→ LockOverlay
         (命名管道通信)
```

---

## 文档清单

| 序号 | 文档名称 | 用途 |
|------|----------|------|
| 01 | 需求规格说明书 | 明确项目需求和验收标准 |
| 02 | 技术设计说明书 | 详细技术实现方案 |
| 03 | 测试方案 | 测试策略和用例设计 |
| 04 | 验收方案 | 验收流程和签字确认 |
| 05 | 用户手册 | 最终用户操作指南 |
| 06 | 部署方案 | 运维人员部署指南 |
