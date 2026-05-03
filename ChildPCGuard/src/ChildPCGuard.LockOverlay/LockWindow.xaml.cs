using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ChildPCGuard.Shared;

namespace ChildPCGuard.LockOverlay
{
    public partial class LockWindow : Window
    {
        private readonly LockReason _lockReason;
        private readonly DispatcherTimer _countdownTimer;
        private DateTime _restEndTime;
        private int _errorCount;
        private DateTime _lockUntil;
        private int _emergencyKeyCount;
        private IntPtr _originalDesktop;
        private IntPtr _lockDesktop;
        private IntPtr _hookId = IntPtr.Zero;
        private readonly LowLevelKeyboardProc _proc;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevMode, uint dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public LockWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && int.TryParse(args[1], out int reason))
            {
                _lockReason = (LockReason)reason;
            }
            else
            {
                _lockReason = LockReason.ManualLock;
            }

            _proc = HookCallback;

            SetupLockScreen();

            if (_lockReason == LockReason.ContinuousLimit)
            {
                StartRestCountdown();
            }
            else
            {
                CountdownText.Visibility = Visibility.Collapsed;
            }

            LoadLockMessage();
        }

        private void SetupLockScreen()
        {
            try
            {
                _originalDesktop = NativeAPI.GetThreadDesktop(NativeAPI.GetCurrentThreadId());

                _lockDesktop = CreateDesktop("LockScreen_" + Guid.NewGuid().ToString("N"),
                    IntPtr.Zero, IntPtr.Zero, 0,
                    NativeAPI.DESKTOP_CREATEWINDOW | NativeAPI.DESKTOP_SWITCHDESKTOP,
                    IntPtr.Zero);

                if (_lockDesktop != IntPtr.Zero)
                {
                    SwitchDesktop(_lockDesktop);
                }

                _hookId = SetWindowsHookEx(13, _proc, GetModuleHandle(null), 0);
            }
            catch { }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                bool isCtrl = GetAsyncKeyState(NativeAPI.VK_LCONTROL) < 0 || GetAsyncKeyState(NativeAPI.VK_RCONTROL) < 0;
                bool isAlt = GetAsyncKeyState(NativeAPI.VK_LALT) < 0 || GetAsyncKeyState(NativeAPI.VK_RALT) < 0;
                bool isShift = GetAsyncKeyState(NativeAPI.VK_LSHIFT) < 0 || GetAsyncKeyState(NativeAPI.VK_RSHIFT) < 0;

                if (isCtrl && isAlt && isShift && vkCode == NativeAPI.VK_F12)
                {
                    _emergencyKeyCount++;
                    if (_emergencyKeyCount >= 5)
                    {
                        ShowEmergencyDialog();
                        _emergencyKeyCount = 0;
                    }
                    return (IntPtr)1;
                }

                bool isWinKey = vkCode == NativeAPI.VK_LWIN || vkCode == NativeAPI.VK_RWIN;
                bool isAltTab = isAlt && vkCode == NativeAPI.VK_TAB;
                bool isCtrlEsc = isCtrl && vkCode == NativeAPI.VK_ESCAPE;

                if (isWinKey || isAltTab || isCtrlEsc)
                {
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void LoadLockMessage()
        {
            try
            {
                string configPath = Path.Combine(@"C:\ProgramData\ChildPCGuard", "config.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var config = JsonSerializer.Deserialize<AppConfiguration>(json, options);
                    if (config != null && !string.IsNullOrEmpty(config.LockScreenMessage))
                    {
                        LockMessage.Text = config.LockScreenMessage;
                    }
                }
            }
            catch { }
        }

        private void StartRestCountdown()
        {
            try
            {
                string configPath = Path.Combine(@"C:\ProgramData\ChildPCGuard", "config.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var config = JsonSerializer.Deserialize<AppConfiguration>(json, options);
                    if (config != null)
                    {
                        _restEndTime = DateTime.Now.AddMinutes(config.RestDurationMinutes);
                    }
                }
            }
            catch { }

            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;
            _countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            var remaining = _restEndTime - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                _countdownTimer.Stop();
                Unlock();
            }
            else
            {
                CountdownText.Text = $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
            }
        }

        private void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lockReason == LockReason.ContinuousLimit)
            {
                ErrorText.Text = "强制休息期间无法解锁，请等待倒计时结束";
                return;
            }

            if (DateTime.Now < _lockUntil)
            {
                var remaining = (_lockUntil - DateTime.Now).TotalSeconds;
                ErrorText.Text = $"锁定中，请 {remaining:F0} 秒后重试";
                return;
            }

            VerifyPassword(PasswordBox.Password);
        }

        private void VerifyPassword(string password)
        {
            try
            {
                string configPath = Path.Combine(@"C:\ProgramData\ChildPCGuard", "config.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var config = JsonSerializer.Deserialize<AppConfiguration>(json, options);
                    if (config != null && !string.IsNullOrEmpty(config.AdminPasswordHash))
                    {
                        var hashedInput = AesEncryption.ComputeHash(password);
                        if (hashedInput == config.AdminPasswordHash)
                        {
                            _errorCount = 0;
                            Unlock();
                            return;
                        }
                    }
                    else if (string.IsNullOrEmpty(config.AdminPasswordHash) && password == "admin")
                    {
                        _errorCount = 0;
                        Unlock();
                        return;
                    }
                }

                _errorCount++;
                if (_errorCount >= 3)
                {
                    _lockUntil = DateTime.Now.AddMinutes(5);
                    ErrorText.Text = "密码错误次数过多，锁定5分钟";
                    _errorCount = 0;
                }
                else
                {
                    ErrorText.Text = $"密码错误，剩余尝试次数 {3 - _errorCount}";
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = "验证失败：" + ex.Message;
            }
        }

        private void Unlock()
        {
            try
            {
                if (_hookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookId);
                }

                if (_lockDesktop != IntPtr.Zero)
                {
                    SwitchDesktop(_originalDesktop);
                    CloseDesktop(_lockDesktop);
                }
            }
            catch { }

            Environment.Exit(0);
        }

        private void ShowEmergencyDialog()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = MessageBox.Show("紧急解锁将通知管理员并暂时解锁电脑。\n是否继续？",
                        "紧急解锁", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        EventLog.WriteEntry("Emergency unlock triggered.", EventLogEntryType.Warning);
                        Unlock();
                    }
                });
            }
            catch { }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
            }

            if (_lockDesktop != IntPtr.Zero)
            {
                CloseDesktop(_lockDesktop);
            }
        }
    }
}
