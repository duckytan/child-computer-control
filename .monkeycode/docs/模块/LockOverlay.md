# ChildPCGuard.LockOverlay

WPF 锁屏界面项目，全屏覆盖显示锁屏画面。

## 项目信息

| 项目 | 值 |
|------|---|
| 项目文件 | `src/ChildPCGuard.LockOverlay/ChildPCGuard.LockOverlay.csproj` |
| 目标框架 | net8.0-windows |
| 输出名称 | LockOverlay.exe |

## 功能

- 全屏锁屏界面
- 根据锁屏原因显示不同提示
- 密码输入验证
- 紧急解锁快捷键支持

## 锁屏界面

### 界面元素

| 元素 | 描述 |
|------|------|
| 背景 | 半透明黑色遮罩 |
| 标题 | 显示锁屏原因 |
| 消息 | 显示锁屏提示文字 |
| 倒计时 | 强制休息剩余时间 |
| 密码框 | 输入管理员密码 |
| 解锁按钮 | 触发密码验证 |
| 错误提示 | 密码错误提示 |

### 锁屏原因对应界面

| LockReason | 标题 | 解锁按钮 | 倒计时 |
|------------|------|----------|--------|
| DailyLimitReached | 使用时间已用完 | 显示 | 无 |
| ContinuousLimit | 需要休息 | 隐藏 | 显示 |
| OutsideAllowedWindow | 不在允许时间段 | 显示 | 无 |
| TimeTampered | 时间异常 | 显示 | 无 |
| ManualLock | 已锁定 | 显示 | 无 |
| AutoShutdown | 即将关机 | 隐藏 | 无 |
| BlockedApp | 程序已阻止 | 显示 | 无 |
| BlockedSite | 网站已阻止 | 显示 | 无 |
| SafeMode | 安全模式 | 隐藏 | 无 |

## 实现

### 锁屏流程

```
GuardService.TriggerLockScreen()
    ↓
创建新桌面 (CreateDesktop)
    ↓
启动 LockOverlay.exe (在新桌面)
    ↓
LockOverlay 在新桌面显示
    ↓
切换桌面 (SwitchDesktop)
```

**代码实现**:

```csharp
// GuardService.cs
public void TriggerLockScreen(LockReason reason)
{
    // 创建新桌面
    IntPtr desktop = NativeAPI.CreateDesktop(
        "LockScreenDesktop", IntPtr.Zero, IntPtr.Zero,
        0, GENERIC_ALL, IntPtr.Zero);

    // 在新桌面启动 LockOverlay
    var startInfo = new PROCESS_INFORMATION();
    NativeAPI.CreateProcess(
        null, $"LockOverlay.exe {reason}",
        IntPtr.Zero, IntPtr.Zero, false,
        CREATE_NEW_CONSOLE | CREATE_NEW_PROCESS_GROUP,
        IntPtr.Zero, null, ref startInfo);

    // 切换到新桌面
    NativeAPI.SwitchDesktop(desktop);
}
```

### 密码验证

```csharp
private void UnlockButton_Click(object sender, RoutedEventArgs e)
{
    // 强制休息期间无法解锁
    if (_lockReason == LockReason.ContinuousLimit)
    {
        ErrorText.Text = "强制休息期间无法解锁，请等待倒计时结束";
        return;
    }

    // 关机时间到无法解锁
    if (_lockReason == LockReason.AutoShutdown)
    {
        ErrorText.Text = "已到达关机时间，无法解锁";
        return;
    }

    // 验证密码
    if (VerifyPassword(PasswordBox.Password))
    {
        Close();
    }
    else
    {
        ErrorText.Text = "密码错误";
    }
}

private bool VerifyPassword(string password)
{
    string hash = AesEncryption.ComputeHash(password);
    return hash == _adminPasswordHash;
}
```

### 解锁流程

```
用户输入密码 → 验证成功
                    ↓
              关闭 LockOverlay
                    ↓
           切换回原桌面 (InputDesktop)
                    ↓
           通知 Service 解锁
```

```csharp
private void Close()
{
    // 切换回原桌面
    IntPtr inputDesktop = NativeAPI.OpenInputDesktop(0, false, GENERIC_ALL);
    NativeAPI.SwitchDesktop(inputDesktop);

    // 关闭自己
    NativeAPI.ExitProcess(0);
}
```

## 使用

```bash
# 根据锁屏原因启动
LockOverlay.exe <LockReason>

# 示例
LockOverlay.exe 1    # DailyLimitReached
LockOverlay.exe 2    # ContinuousLimit
```

## 依赖

```xml
<ItemGroup>
    <ProjectReference Include="..\ChildPCGuard.Shared\ChildPCGuard.Shared.csproj" />
</ItemGroup>
```

## 注意事项

- LockOverlay 运行在独立的桌面，防止被任务管理器结束
- 使用 WPF 的 WindowStyle=None 和 WindowState=Maximized 实现全屏
- Topmost 属性确保界面在最顶层
