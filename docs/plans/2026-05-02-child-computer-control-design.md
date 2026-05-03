# 儿童电脑使用时间控制程序 - 设计文档

**创建时间**: 2026-05-02
**版本**: 1.0

---

## 一、功能需求

| 需求 | 描述 |
|------|------|
| 时长限制 | 每日总时长可配置，默认180分钟 |
| 强制休息 | 每连续玩45分钟，强制休息5分钟，时间均可配置 |
| 定时关机 | 每天22:00自动关机 |
| 配置访问 | 通过CMD指令和远程CMD指令管理 |
| 进程保护 | 双进程互相守护 + Service监控 |
| 完全隐藏 | 进程伪装成系统进程，无窗口无托盘 |
| 监控记录 | 记录程序和网站使用情况 |
| 黑名单 | 可禁止特定程序和网站，触发后立即锁屏 |
| 解锁方式 | 家长密码解锁，强制休息需等倒计时结束 |

---

## 二、系统架构

### 2.1 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                    ChildControlService (SYSTEM权限)         │
│    Windows服务，开机自启，核心监控引擎                       │
│    ├── TimeTracker      使用时间统计                        │
│    ├── ProcessGuardian  进程守护(防结束)                    │
│    ├── AppMonitor       程序使用监控                         │
│    ├── WebMonitor       网站访问监控                        │
│    └── ShutdownScheduler 定时关机调度                       │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
        ┌────────┐      ┌────────┐      ┌──────────┐
        │AgentA  │◄────►│AgentB  │      │ LockScreen│
        │(伪装)  │      │(伪装)  │      │ (锁屏界面)│
        └────────┘      └────────┘      └──────────┘
```

### 2.2 组件说明

| 组件 | 类型 | 职责 |
|------|------|------|
| ChildControlService | Windows服务(SYSTEM) | 核心监控，进程管理，定时任务 |
| AgentA.exe | 用户进程(伪装svchost) | 守护进程A，与B互相监控 |
| AgentB.exe | 用户进程(伪装RuntimeBroker) | 守护进程B，与A互相监控 |
| LockScreen.exe | 用户进程 | 显示锁屏界面，验证密码 |
| config.exe | 用户进程 | CMD配置文件程序 |

---

## 三、时间管理体系

### 3.1 配置参数

```json
{
  "dailyLimitMinutes": 180,
  "continuousLimitMinutes": 45,
  "restDurationMinutes": 5,
  "shutdownTime": "22:00"
}
```

### 3.2 状态机

```
[使用中] ─45分钟到→ [强制休息] ─5分钟→ [可继续使用] ─...→ [每日总时长到] → [锁定]
                                    ↑
                               [22:00关机]
```

- 强制休息期间无法通过密码解锁
- 每日总时长用尽后可解锁
- 关机时间到时无法解锁

---

## 四、监控与黑名单

### 4.1 程序监控

- 定期采集前台窗口进程信息（进程名、路径、启动时间）
- 按日存储到 `logs/YYYY-MM-DD_process.json`

### 4.2 网站监控

- 通过浏览器历史记录或网络日志抓取
- 存储到 `logs/YYYY-MM-DD_web.json`

### 4.3 黑名单

```json
{
  "blockedApps": ["game1.exe", "game2.exe"],
  "blockedSites": ["youtube.com", "twitch.tv"]
}
```

- 检测到黑名单程序运行 → 立即锁屏
- 检测到黑名单网站访问 → 立即锁屏

---

## 五、进程保护机制

### 5.1 进程伪装

| 原进程 | 伪装目标 |
|--------|----------|
| AgentA.exe | svchost.exe (System32) |
| AgentB.exe | RuntimeBroker.exe (System32) |

### 5.2 三层守护

```
Layer 1: AgentA ↔ AgentB 互相监控
Layer 2: Service接收心跳，超10秒无心跳则重启
Layer 3: Service配置自动重启
```

### 5.3 静默运行

- 无窗口、无托盘图标
- 所有通信通过Named Pipe

---

## 六、锁屏与解锁

### 6.1 触发条件

1. 每日总时长用尽
2. 连续使用时间到（强制休息）
3. 运行黑名单程序
4. 访问黑名单网站
5. 定时关机时间到达

### 6.2 解锁规则

| 场景 | 解锁方式 |
|------|----------|
| 总时长用尽 | 家长密码解锁 |
| 强制休息 | 倒计时结束自动解锁 |
| 黑名单触发 | 家长密码解锁 |
| 关机时间到 | 无法解锁，强制关机 |

### 6.3 错误处理

- 5分钟内连续3次密码错误 → 锁定5分钟

### 6.4 紧急解锁

- 连续按 `Ctrl+Alt+Shift+F12` 5次 → 弹出紧急对话框

---

## 七、数据存储

### 7.1 目录结构

```
C:\ProgramData\ChildControl\
├── config.json           # 主配置文件
├── logs\
│   ├── YYYY-MM-DD_process.json
│   └── YYYY-MM-DD_web.json
└── data\
    └── usage_stats.json  # 累计统计
```

### 7.2 配置文件

```json
{
  "dailyLimitMinutes": 180,
  "continuousLimitMinutes": 45,
  "restDurationMinutes": 5,
  "shutdownTime": "22:00",
  "unlockPassword": "hashed_password",
  "isEnabled": true,
  "blockedApps": [],
  "blockedSites": []
}
```

---

## 八、CMD命令接口

```bash
config.exe --view              # 查看当前配置
config.exe --set dailyLimit 120
config.exe --set continuousLimit 30
config.exe --set restDuration 10
config.exe --set shutdownTime "21:00"
config.exe --block-app game.exe
config.exe --unblock-app game.exe
config.exe --block-site youtube.com
config.exe --unblock-site youtube.com
config.exe --view-logs today
config.exe --reset-daily       # 重置当日使用时间
```

---

## 九、部署

### 9.1 进程文件

```
C:\Windows\System32\
├── svchost.exe          # AgentA (伪装)
├── RuntimeBroker.exe    # AgentB (伪装)
└── config.exe           # 配置文件程序
```

### 9.2 服务伪装

- 服务名：`RemoteAccessService`
- 显示名：`Windows Remote Access`

### 9.3 安装

管理员权限运行安装脚本，自动完成复制、注册、启动。

### 9.4 卸载

```bash
uninstall.exe --password <密码>
```

或安全模式下删除相关文件。

---

## 十、技术选型

| 组件 | 技术 |
|------|------|
| 语言 | C# .NET 8 |
| 架构 | Windows Service + 用户态进程 |
| 进程通信 | Named Pipe |
| 锁屏 | Win32 API (CreateDesktop/SwitchDesktop) |
| 时间统计 | GetLastInputInfo |
| 配置存储 | JSON + 注册表备份 |

---

*本文档已通过用户确认*
