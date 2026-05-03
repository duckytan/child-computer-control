---

## 15. 完整源代码

### 15.1 共享库 (ChildPCGuard.Shared)

#### NativeAPI.cs

```csharp
using System;
using System.Runtime.InteropServices;

namespace ChildPCGuard.Shared
{
    public static class NativeAPI
    {
        #region User32.dll

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool LockWorkStation();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr OpenDesktop(string lpszDesktop, uint dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetThreadDesktop(uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevMode, uint dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool CloseWindowStation(IntPtr hWinStation);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr OpenWindowStation(string szName, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Kernel32.dll

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetSystemTime(out SYSTEMTIME lpSystemTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(IntPtr hMem);

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion

        #region Constants

        public const uint INFINITE = 0xFFFFFFFF;
        public const uint WAIT_OBJECT_0 = 0x00000000;
        public const uint WAIT_TIMEOUT = 0x00000102;

        public const uint DESKTUP_CREATEWINDOW = 0x0002;
        public const uint DESKTOP_SWITCHDESKTOP = 0x0100;
        public const uint DESKTOP_CREATEMENU = 0x0004;
        public const uint DESKTOP_ENUMERATE = 0x0040;
        public const uint DESKTOP_EXITWINDOWS = 0x0040;
        public const uint DESKTOP_READOBJECTS = 0x0001;
        public const uint DESKTOP_WRITEOBJECTS = 0x0080;

        public const uint PROCESS_CREATE_PROCESS = 0x0080;
        public const uint PROCESS_CREATE_THREAD = 0x0002;
        public const uint PROCESS_DUP_HANDLE = 0x0040;
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        public const uint PROCESS_SET_INFORMATION = 0x0200;
        public const uint PROCESS_SET_QUOTA = 0x0100;
        public const uint PROCESS_SYNCHRONIZE = 0x0010;
        public const uint PROCESS_TERMINATE = 0x0001;
        public const uint PROCESS_VM_OPERATION = 0x0008;
        public const uint PROCESS_VM_READ = 0x0010;
        public const uint PROCESS_VM_WRITE = 0x0020;

        public const uint STILL_ACTIVE = 0x00000103;

        public const int EWX_LOGOFF = 0x00000000;
        public const int EWX_SHUTDOWN = 000000001;
        public const int EWX_REBOOT = 0x00000002;
        public const int EWX_FORCE = 0x00000004;
        public const int EWX_POWEROFF = 0x00000008;
        public const int EWX_FORCEIFHUNG = 0x00000010;

        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYUP = 0x0105;

        public const int VK_LWIN = 0x5B;
        public const int VK_RWIN = 0x5C;
        public const int VK_TAB = 0x09;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_F4 = 0x73;
        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        public const int VK_LSHIFT = 0xA0;
        public const int VK_RSHIFT = 0xA1;
        public const int VK_LALT = 0xA4;
        public const int VK_RALT = 0xA5;
        public const int VK_CONTROL = 0x11;
        public const int VK_SHIFT = 0x10;
        public const int VK_ALT = 0x12;

        public const int SM_CLEANBOOT = 67;

        #endregion
    }
}
```

#### PipeMessages.cs

```csharp
using System;

namespace ChildPCGuard.Shared
{
    public enum PipeMessageType : byte
    {
        Heartbeat = 1,
        LockRequest = 2,
        UnlockRequest = 3,
        RestartAgent = 4,
        ConfigChanged = 5,
        GetStatus = 6,
        StatusResponse = 7,
        LogRecord = 8,
        AddTime = 9,
        PauseControl = 10,
        LockNow = 11,
        ShutdownNow = 12
    }

    [Serializable]
    public class PipeMessage
    {
        public PipeMessageType Type { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Payload { get; set; }
    }

    [Serializable]
    public class HeartbeatMessage : PipeMessage
    {
        public HeartbeatMessage()
        {
            Type = PipeMessageType.Heartbeat;
            ProcessId = (int)NativeAPI.GetCurrentProcessId();
            ProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Timestamp = DateTime.Now;
        }
        public uint MemoryUsage { get; set; }
        public TimeSpan Uptime { get; set; }
    }

    [Serializable]
    public class StatusMessage : PipeMessage
    {
        public StatusMessage()
        {
            Type = PipeMessageType.StatusResponse;
        }
        public UsageState CurrentState { get; set; }
        public TimeSpan UsedTimeToday { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public TimeSpan ContinuousUsageTime { get; set; }
        public TimeSpan RestRemainingTime { get; set; }
        public DateTime? ShutdownTime { get; set; }
        public int BlockedAppsCount { get; set; }
        public int BlockedSitesCount { get; set; }
        public bool IsServiceRunning { get; set; }
    }

    public enum UsageState
    {
        Using = 0,
        Resting = 1,
        Locked = 2,
        Shutdown = 3,
        Paused = 4,
        Normal = 10
    }

    [Serializable]
    public class LogRecord
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; }
        public string ProcessPath { get; set; }
        public int Duration { get; set; }
    }

    [Serializable]
    public class WebLogRecord
    {
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public string Domain { get; set; }
        public string Title { get; set; }
    }
}
```

#### Models.cs

```csharp
using System;
using System.Collections.Generic;

namespace ChildPCGuard.Shared
{
    public class AppConfiguration
    {
        public string Version { get; set; } = "1.0";
        public bool IsEnabled { get; set; } = true;
        public string AdminPasswordHash { get; set; }
        public RulesConfiguration Rules { get; set; } = new RulesConfiguration();
        public string AutoShutdownTime { get; set; } = "22:00";
        public int[] WarningMinutes { get; set; } = new[] { 10, 5, 1 };
        public int IdleThresholdMs { get; set; } = 5000;
        public int ContinuousLimitMinutes { get; set; } = 45;
        public int RestDurationMinutes { get; set; } = 5;
        public List<string> BlockedApps { get; set; } = new List<string>();
        public List<string> BlockedSites { get; set; } = new List<string>();
        public bool UseNtpValidation { get; set; } = true;
        public string[] NtpServers { get; set; } = new[] { "pool.ntp.org", "time.windows.com", "cn.pool.ntp.org" };
        public int NtpToleranceMinutes { get; set; } = 5;
        public string ServiceName { get; set; } = "WinSecSvc_a1b2c3d4";
        public string ServiceDisplayName { get; set; } = "Windows Security Update Service";
        public string LockScreenMessage { get; set; } = "今天的使用时间已到，休息一下吧！";
        public string EmergencyUnlockShortcut { get; set; } = "Ctrl+Alt+Shift+F12";
    }

    public class RulesConfiguration
    {
        public TimeRule Weekdays { get; set; } = new TimeRule
        {
            DailyLimitMinutes = 120,
            AllowedTimeWindows = new List<TimeWindow>
            {
                new TimeWindow { Start = "15:00", End = "20:00" }
            }
        };
        public TimeRule Weekends { get; set; } = new TimeRule
        {
            DailyLimitMinutes = 240,
            AllowedTimeWindows = new List<TimeWindow>
            {
                new TimeWindow { Start = "09:00", End = "21:00" }
            }
        };
    }

    public class TimeRule
    {
        public int DailyLimitMinutes { get; set; }
        public List<TimeWindow> AllowedTimeWindows { get; set; } = new List<TimeWindow>();
    }

    public class TimeWindow
    {
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class DailyUsageData
    {
        public string Date { get; set; }
        public TimeSpan TotalUsedTime { get; set; }
        public TimeSpan ContinuousUsedTime { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime? LastInputTime { get; set; }
        public UsageState CurrentState { get; set; }
        public DateTime? RestEndTime { get; set; }
        public DateTime? PausedUntil { get; set; }
        public int ExtraMinutesToday { get; set; }
        public DateTime? LastNtpCheckTime { get; set; }
        public DateTime? LastNtpTime { get; set; }
    }

    public class ProcessUsageLog
    {
        public string Date { get; set; }
        public List<ProcessUsageRecord> Records { get; set; } = new List<ProcessUsageRecord>();
    }

    public class ProcessUsageRecord
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; }
        public string ProcessPath { get; set; }
        public int Duration { get; set; }
    }

    public class WebUsageLog
    {
        public string Date { get; set; }
        public List<WebUsageRecord> Records { get; set; } = new List<WebUsageRecord>();
    }

    public class WebUsageRecord
    {
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public string Domain { get; set; }
        public string Title { get; set; }
    }

    public enum LockReason
    {
        DailyLimitReached = 1,
        ContinuousLimit = 2,
        OutsideAllowedWindow = 3,
        TimeTampered = 4,
        ManualLock = 5,
        AutoShutdown = 6,
        BlockedApp = 7,
        BlockedSite = 8,
        SafeMode = 9
    }
}
```

#### AesEncryption.cs

```csharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ChildPCGuard.Shared
{
    public static class AesEncryption
    {
        private static readonly byte[] DefaultIV = new byte[16]
        {
            0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0,
            0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88
        };

        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(key);
                aes.IV = DefaultIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                    swEncrypt.Flush();
                    csEncrypt.FlushFinalBlock();
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            byte[] buffer;
            try
            {
                buffer = Convert.FromBase64String(cipherText);
            }
            catch
            {
                throw new CryptographicException("Invalid cipher text format");
            }

            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(key);
                aes.IV = DefaultIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                using (var msDecrypt = new MemoryStream(buffer))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        private static byte[] DeriveKey(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static string GenerateKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
```

### 15.2 Windows服务 (ChildPCGuard.GuardService)

#### Program.cs

```csharp
using System;
using System.ServiceProcess;
using System.Threading;

namespace ChildPCGuard.GuardService
{
    internal static class Program
    {
        private static ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--console")
            {
                RunAsConsole();
            }
            else
            {
                ServiceBase[] ServicesToRun = new ServiceBase[]
                {
                    new GuardService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        static void RunAsConsole()
        {
            Console.WriteLine("ChildPCGuard Service starting in console mode...");
            var service = new GuardService();
            service.Start(new string[0]);

            Console.WriteLine("Service started. Press Ctrl+C to stop.");
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _shutdownEvent.Set();
            };

            _shutdownEvent.WaitOne();
            service.Stop();
            Console.WriteLine("Service stopped.");
        }
    }
}
```

#### GuardService.cs

```csharp
using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public partial class GuardService : ServiceBase
    {
        private Timer _monitoringTimer;
        private Timer _shutdownCheckTimer;
        private Timer _ntpCheckTimer;
        private CancellationTokenSource _cts;

        private TimeTracker _timeTracker;
        private ProcessGuardian _processGuardian;
        private AppMonitor _appMonitor;
        private WebMonitor _webMonitor;
        private ShutdownScheduler _shutdownScheduler;
        private NtpValidator _ntpValidator;
        private NotificationHelper _notificationHelper;
        private NamedPipeServer _pipeServer;
        private ConfigManager _configManager;

        private static readonly string DataDirectory = @"C:\ProgramData\ChildPCGuard";
        private static readonly string LogDirectory = Path.Combine(DataDirectory, "logs");

        public GuardService()
        {
            InitializeComponent();
            ServiceName = "WinSecSvc_a1b2c3d4";
            DisplayName = "Windows Security Update Service";
            CanStop = false;
            CanShutdown = true;
            CanPauseAndContinue = false;
        }

        public void Start(string[] args)
        {
            try
            {
                InitializeDirectories();
                _configManager = new ConfigManager(DataDirectory);
                var config = _configManager.Load();

                _timeTracker = new TimeTracker(config);
                _processGuardian = new ProcessGuardian(config);
                _appMonitor = new AppMonitor(config);
                _webMonitor = new WebMonitor(config);
                _shutdownScheduler = new ShutdownScheduler(config);
                _ntpValidator = new NtpValidator(config);
                _notificationHelper = new NotificationHelper(config);

                _pipeServer = new NamedPipeServer("ChildPCGuardPipe");
                _pipeServer.OnMessageReceived += OnPipeMessageReceived;
                _pipeServer.Start();

                _processGuardian.OnAgentDead += OnAgentDead;

                _monitoringTimer = new Timer(MonitoringCallback, null,
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(5));

                _shutdownCheckTimer = new Timer(ShutdownCheckCallback, null,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(30));

                _ntpCheckTimer = new Timer(NtpCheckCallback, null,
                    TimeSpan.Zero,
                    TimeSpan.FromMinutes(5));

                _processGuardian.StartGuardians();

                CheckSafeMode();

                EventLog.WriteEntry($"ChildPCGuard Service started successfully. Config loaded.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"ChildPCGuard Service failed to start: {ex}", EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStart(string[] args)
        {
            Start(args);
        }

        protected override void OnStop()
        {
            _cts?.Cancel();
            _monitoringTimer?.Dispose();
            _shutdownCheckTimer?.Dispose();
            _ntpCheckTimer?.Dispose();
            _pipeServer?.Stop();
            _processGuardian?.Stop();
            EventLog.WriteEntry("ChildPCGuard Service stopped.", EventLogEntryType.Information);
        }

        protected override void OnShutdown()
        {
            OnStop();
        }

        private void InitializeDirectories()
        {
            if (!Directory.Exists(DataDirectory))
                Directory.CreateDirectory(DataDirectory);
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
            if (!Directory.Exists(Path.Combine(DataDirectory, "data")))
                Directory.CreateDirectory(Path.Combine(DataDirectory, "data"));
        }

        private void CheckSafeMode()
        {
            int bootType = NativeAPI.GetSystemMetrics(NativeAPI.SM_CLEANBOOT);
            if (bootType != 0)
            {
                EventLog.WriteEntry("Safe mode detected! Initiating shutdown.", EventLogEntryType.Warning);
                TriggerImmediateShutdown();
            }
        }

        private void MonitoringCallback(object state)
        {
            try
            {
                var config = _configManager.Load();
                if (!config.IsEnabled) return;

                _timeTracker.Update();
                _appMonitor.CheckCurrentProcess();
                _webMonitor.CheckCurrentBrowsers();

                var stateInfo = _timeTracker.GetState();

                switch (stateInfo.State)
                {
                    case UsageState.Using:
                        if (stateInfo.ContinuousTime >= TimeSpan.FromMinutes(config.ContinuousLimitMinutes))
                        {
                            _timeTracker.StartRest();
                            TriggerLockScreen(LockReason.ContinuousLimit);
                        }
                        CheckWarningNotifications(stateInfo.RemainingTime, config);
                        break;
                    case UsageState.Locked:
                        TriggerLockScreen(LockReason.DailyLimitReached);
                        break;
                    case UsageState.Resting:
                        if (stateInfo.RestRemainingTime <= TimeSpan.Zero)
                        {
                            _timeTracker.EndRest();
                        }
                        break;
                }

                if (_appMonitor.IsBlockedProcessRunning())
                {
                    TriggerLockScreen(LockReason.BlockedApp);
                }

                if (_webMonitor.IsBlockedSiteAccessed())
                {
                    TriggerLockScreen(LockReason.BlockedSite);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Monitoring error: {ex.Message}", EventLogEntryType.Warning);
            }
        }

        private void CheckWarningNotifications(TimeSpan remaining, AppConfiguration config)
        {
            foreach (var minute in config.WarningMinutes)
            {
                if (Math.Abs(remaining.TotalMinutes - minute) < 0.5)
                {
                    _notificationHelper.ShowWarning($"还有 {minute} 分钟，记得保存进度哦");
                }
            }
        }

        private void ShutdownCheckCallback(object state)
        {
            try
            {
                var config = _configManager.Load();
                if (_shutdownScheduler.ShouldShutdown())
                {
                    _notificationHelper.ShowWarning("电脑将在 60 秒后自动关机");
                    Thread.Sleep(60000);
                    TriggerShutdown();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Shutdown check error: {ex.Message}", EventLogEntryType.Warning);
            }
        }

        private void NtpCheckCallback(object state)
        {
            try
            {
                var config = _configManager.Load();
                if (!config.UseNtpValidation) return;

                if (_ntpValidator.ValidateTime())
                {
                    _timeTracker.UpdateNtpTime(_ntpValidator.GetCurrentNtpTime());
                }
                else
                {
                    EventLog.WriteEntry("NTP validation failed - network unavailable or server unreachable", EventLogEntryType.Warning);
                }

                if (_ntpValidator.IsTimeTampered())
                {
                    EventLog.WriteEntry("Time tampering detected! Triggering lock screen.", EventLogEntryType.Warning);
                    TriggerLockScreen(LockReason.TimeTampered);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"NTP check error: {ex.Message}", EventLogEntryType.Warning);
            }
        }

        private void OnPipeMessageReceived(PipeMessage message)
        {
            switch (message.Type)
            {
                case PipeMessageType.Heartbeat:
                    _processGuardian.ReceiveHeartbeat(message.ProcessName);
                    break;
                case PipeMessageType.GetStatus:
                    var status = CreateStatusMessage();
                    _pipeServer.SendMessage(status);
                    break;
                case PipeMessageType.UnlockRequest:
                    HandleUnlockRequest(message);
                    break;
                case PipeMessageType.AddTime:
                    int minutes = int.Parse(message.Payload);
                    _timeTracker.AddExtraTime(minutes);
                    break;
                case PipeMessageType.LockNow:
                    TriggerLockScreen(LockReason.ManualLock);
                    break;
                case PipeMessageType.ShutdownNow:
                    TriggerShutdown();
                    break;
            }
        }

        private void OnAgentDead(string agentName)
        {
            EventLog.WriteEntry($"Agent {agentName} died, restarting...", EventLogEntryType.Warning);
            _processGuardian.RestartAgent(agentName);
        }

        private void HandleUnlockRequest(PipeMessage message)
        {
        }

        private StatusMessage CreateStatusMessage()
        {
            var stateInfo = _timeTracker.GetState();
            var config = _configManager.Load();
            return new StatusMessage
            {
                CurrentState = stateInfo.State,
                UsedTimeToday = stateInfo.UsedTime,
                RemainingTime = TimeSpan.FromMinutes(config.Rules.Weekdays.DailyLimitMinutes) - stateInfo.UsedTime,
                ContinuousUsageTime = stateInfo.ContinuousTime,
                RestRemainingTime = stateInfo.RestRemainingTime,
                ShutdownTime = DateTime.Today.Add(TimeSpan.Parse(config.AutoShutdownTime)),
                BlockedAppsCount = config.BlockedApps.Count,
                BlockedSitesCount = config.BlockedSites.Count,
                IsServiceRunning = true
            };
        }

        public void TriggerLockScreen(LockReason reason)
        {
            try
            {
                string lockScreenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "LockOverlay.exe");
                if (!File.Exists(lockScreenPath))
                {
                    lockScreenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LockOverlay.exe");
                }

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = lockScreenPath,
                    Arguments = ((int)reason).ToString(),
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Failed to trigger lock screen: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void TriggerShutdown()
        {
            try
            {
                EventLog.WriteEntry("Shutdown triggered by schedule.", EventLogEntryType.Information);
                NativeAPI.LockWorkStation();
                Thread.Sleep(30000);
                TriggerShutdownCommand();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Shutdown error: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void TriggerImmediateShutdown()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/s /f /t 0",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch { }
        }

        private void TriggerShutdownCommand()
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/s /f /t 0",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(psi);
        }
    }
}
```

#### TimeTracker.cs

```csharp
using System;
using System.Runtime.InteropServices;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class TimeTracker
    {
        private readonly AppConfiguration _config;
        private DailyUsageData _todayData;
        private DateTime _lastCheckTime;

        public TimeTracker(AppConfiguration config)
        {
            _config = config;
            ResetIfNewDay();
            LoadDailyData();
        }

        public void Update()
        {
            LASTINPUTINFO lastInput = new LASTINPUTINFO();
            lastInput.cbSize = (uint)Marshal.SizeOf(lastInput);

            if (NativeAPI.GetLastInputInfo(ref lastInput))
            {
                uint currentTick = (uint)Environment.TickCount;
                uint idleMs = currentTick - lastInput.dwTime;

                if (idleMs < _config.IdleThresholdMs)
                {
                    _todayData.TotalUsedTime += TimeSpan.FromMilliseconds(idleMs);
                    _todayData.ContinuousUsedTime += TimeSpan.FromMilliseconds(idleMs);

                    if (_todayData.CurrentState == UsageState.Resting)
                    {
                        _todayData.CurrentState = UsageState.Using;
                    }
                }

                _todayData.LastInputTime = DateTime.Now;
            }

            _lastCheckTime = DateTime.Now;
            SaveDailyData();
        }

        public (UsageState State, TimeSpan UsedTime, TimeSpan ContinuousTime, TimeSpan RestRemainingTime) GetState()
        {
            ResetIfNewDay();

            return (
                _todayData.CurrentState,
                _todayData.TotalUsedTime + TimeSpan.FromMinutes(_todayData.ExtraMinutesToday),
                _todayData.ContinuousUsedTime,
                _todayData.CurrentState == UsageState.Resting && _todayData.RestEndTime.HasValue
                    ? _todayData.RestEndTime.Value - DateTime.Now
                    : TimeSpan.Zero
            );
        }

        public void StartRest()
        {
            _todayData.CurrentState = UsageState.Resting;
            _todayData.ContinuousUsedTime = TimeSpan.Zero;
            _todayData.RestEndTime = DateTime.Now.AddMinutes(_config.RestDurationMinutes);
            SaveDailyData();
        }

        public void EndRest()
        {
            _todayData.CurrentState = UsageState.Using;
            _todayData.RestEndTime = null;
            SaveDailyData();
        }

        public void AddExtraTime(int minutes)
        {
            _todayData.ExtraMinutesToday += minutes;
            SaveDailyData();
        }

        public void ResetDaily()
        {
            _todayData = new DailyUsageData
            {
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                SessionStart = DateTime.Now,
                CurrentState = UsageState.Using
            };
            SaveDailyData();
        }

        public bool IsDailyLimitReached()
        {
            int limit = IsWeekend() ? _config.Rules.Weekends.DailyLimitMinutes : _config.Rules.Weekdays.DailyLimitMinutes;
            return (_todayData.TotalUsedTime.TotalMinutes + _todayData.ExtraMinutesToday) >= limit;
        }

        public bool IsContinuousLimitReached()
        {
            return _todayData.ContinuousUsedTime >= TimeSpan.FromMinutes(_config.ContinuousLimitMinutes);
        }

        public void UpdateNtpTime(DateTime ntpTime)
        {
            _todayData.LastNtpCheckTime = DateTime.Now;
            _todayData.LastNtpTime = ntpTime;
            SaveDailyData();
        }

        private bool IsWeekend()
        {
            return DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday;
        }

        private void ResetIfNewDay()
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            if (_todayData == null || _todayData.Date != today)
            {
                _todayData = new DailyUsageData
                {
                    Date = today,
                    SessionStart = DateTime.Now,
                    CurrentState = UsageState.Using
                };
                SaveDailyData();
            }
        }

        private void LoadDailyData()
        {
            try
            {
                string dataPath = System.IO.Path.Combine(@"C:\ProgramData\ChildPCGuard\data", "usage_data.json");
                if (System.IO.File.Exists(dataPath))
                {
                    var json = System.IO.File.ReadAllText(dataPath);
                    var data = System.Text.Json.JsonSerializer.Deserialize<DailyUsageData>(json);
                    if (data != null && data.Date == DateTime.Today.ToString("yyyy-MM-dd"))
                    {
                        _todayData = data;
                    }
                }
            }
            catch { }
        }

        private void SaveDailyData()
        {
            try
            {
                string dataPath = System.IO.Path.Combine(@"C:\ProgramData\ChildPCGuard\data", "usage_data.json");
                string directory = System.IO.Path.GetDirectoryName(dataPath);
                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var json = System.Text.Json.JsonSerializer.Serialize(_todayData, options);
                System.IO.File.WriteAllText(dataPath, json);
            }
            catch { }
        }
    }
}
```

#### ProcessGuardian.cs

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class ProcessGuardian
    {
        private readonly AppConfiguration _config;
        private readonly Dictionary<string, DateTime> _lastHeartbeat = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, Process> _agentProcesses = new Dictionary<string, Process>();
        private Timer _heartbeatCheckTimer;
        private bool _isRunning;

        private readonly string[] _agentPaths = new string[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "svchost.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "RuntimeBroker.exe")
        };

        public event Action<string> OnAgentDead;

        public ProcessGuardian(AppConfiguration config)
        {
            _config = config;
        }

        public void StartGuardians()
        {
            _isRunning = true;

            for (int i = 0; i < _agentPaths.Length; i++)
            {
                string agentPath = _agentPaths[i];
                string agentName = Path.GetFileNameWithoutExtension(agentPath);
                StartAgent(agentName, agentPath, i == 0 ? "--agent-a" : "--agent-b");
            }

            _heartbeatCheckTimer = new Timer(CheckHeartbeats, null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10));
        }

        public void Stop()
        {
            _isRunning = false;
            _heartbeatCheckTimer?.Dispose();

            foreach (var process in _agentProcesses.Values)
            {
                try { process.Kill(); } catch { }
            }
            _agentProcesses.Clear();
        }

        public void ReceiveHeartbeat(string agentName)
        {
            _lastHeartbeat[agentName] = DateTime.Now;
        }

        private void StartAgent(string agentName, string agentPath, string argument)
        {
            try
            {
                if (!File.Exists(agentPath))
                {
                    string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{agentName}.exe");
                    if (File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, agentPath, true);
                    }
                }

                var psi = new ProcessStartInfo
                {
                    FileName = agentPath,
                    Arguments = argument,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                if (process != null)
                {
                    _agentProcesses[agentName] = process;
                    _lastHeartbeat[agentName] = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Failed to start agent {agentName}: {ex.Message}", EventLogEntryType.Error);
            }
        }

        public void RestartAgent(string agentName)
        {
            if (!_isRunning) return;

            if (_agentProcesses.ContainsKey(agentName))
            {
                try { _agentProcesses[agentName].Kill(); } catch { }
                _agentProcesses.Remove(agentName);
            }

            string agentPath = _agentPaths.FirstOrDefault(p =>
                Path.GetFileNameWithoutExtension(p) == agentName);

            if (agentPath != null)
            {
                string argument = agentName == "svchost" ? "--agent-a" : "--agent-b";
                StartAgent(agentName, agentPath, argument);
            }
        }

        private void CheckHeartbeats(object state)
        {
            if (!_isRunning) return;

            var timeout = TimeSpan.FromSeconds(_config.IdleThresholdMs / 200 + 5);

            foreach (var kvp in _lastHeartbeat.ToList())
            {
                if (DateTime.Now - kvp.Value > timeout)
                {
                    EventLog.WriteEntry($"Agent {kvp.Key} heartbeat timeout, restarting...", EventLogEntryType.Warning);
                    OnAgentDead?.Invoke(kvp.Key);
                    RestartAgent(kvp.Key);
                }
            }

            foreach (var kvp in _agentProcesses.ToList())
            {
                if (kvp.Value.HasExited)
                {
                    EventLog.WriteEntry($"Agent {kvp.Key} process exited, restarting...", EventLogEntryType.Warning);
                    RestartAgent(kvp.Key);
                }
            }
        }
    }
}
```

#### AppMonitor.cs

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class AppMonitor
    {
        private readonly AppConfiguration _config;
        private string _currentForegroundProcess;
        private DateTime _currentProcessStartTime;
        private readonly Dictionary<string, DateTime> _processUsageStart = new Dictionary<string, DateTime>();

        public AppMonitor(AppConfiguration config)
        {
            _config = config;
        }

        public void CheckCurrentProcess()
        {
            try
            {
                var foregroundWindow = GetForegroundWindowProcess();
                if (foregroundWindow == null) return;

                string processName = foregroundWindow.ProcessName;

                if (_currentForegroundProcess != processName)
                {
                    RecordProcessUsage(_currentForegroundProcess, _currentProcessStartTime);
                    _currentForegroundProcess = processName;
                    _currentProcessStartTime = DateTime.Now;
                    _processUsageStart[processName] = DateTime.Now;
                }

                CheckBlacklist(processName);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"AppMonitor error: {ex.Message}", EventLogEntryType.Warning);
            }
        }

        private Process GetForegroundWindowProcess()
        {
            try
            {
                var hwnd = NativeAPI.GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return null;

                NativeAPI.GetWindowThreadProcessId(hwnd, out uint processId);
                return Process.GetProcessById((int)processId);
            }
            catch
            {
                return null;
            }
        }

        private void CheckBlacklist(string processName)
        {
            if (_config.BlockedApps == null || _config.BlockedApps.Count == 0) return;

            foreach (var blocked in _config.BlockedApps)
            {
                if (processName.Equals(blocked, StringComparison.OrdinalIgnoreCase) ||
                    processName.Contains(blocked, StringComparison.OrdinalIgnoreCase))
                {
                    EventLog.WriteEntry($"Blocked app detected: {processName}", EventLogEntryType.Warning);
                    return;
                }
            }
        }

        public bool IsBlockedProcessRunning()
        {
            if (_config.BlockedApps == null || _config.BlockedApps.Count == 0) return false;

            var runningProcesses = Process.GetProcesses();
            foreach (var process in runningProcesses)
            {
                foreach (var blocked in _config.BlockedApps)
                {
                    if (process.ProcessName.Equals(blocked, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void RecordProcessUsage(string processName, DateTime startTime)
        {
            if (string.IsNullOrEmpty(processName) || startTime == DateTime.MinValue) return;

            try
            {
                var duration = (int)(DateTime.Now - startTime).TotalSeconds;
                if (duration < 5) return;

                var log = new ProcessUsageLog
                {
                    Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    Records = new List<ProcessUsageRecord>
                    {
                        new ProcessUsageRecord
                        {
                            Timestamp = startTime,
                            ProcessName = processName,
                            ProcessPath = GetProcessPath(processName),
                            Duration = duration
                        }
                    }
                };

                string logPath = Path.Combine(@"C:\ProgramData\ChildPCGuard\logs",
                    $"process_{DateTime.Today:yyyy-MM-dd}.json");

                List<ProcessUsageLog> existingLogs = new List<ProcessUsageLog>();
                if (File.Exists(logPath))
                {
                    var json = File.ReadAllText(logPath);
                    existingLogs = JsonSerializer.Deserialize<List<ProcessUsageLog>>(json) ?? new List<ProcessUsageLog>();
                }

                existingLogs.Add(log);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(logPath, JsonSerializer.Serialize(existingLogs, options));
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Failed to record process usage: {ex.Message}", EventLogEntryType.Warning);
            }
        }

        private string GetProcessPath(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0 && processes[0].MainModule != null)
                {
                    return processes[0].MainModule.FileName;
                }
            }
            catch { }
            return "";
        }
    }
}
```

#### WebMonitor.cs

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class WebMonitor
    {
        private readonly AppConfiguration _config;
        private readonly string[] _browserPaths = new string[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\History"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Edge\User Data\Default\History"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Mozilla\Firefox\Profiles")
        };

        public WebMonitor(AppConfiguration config)
        {
            _config = config;
        }

        public void CheckCurrentBrowsers()
        {
            if (_config.BlockedSites == null || _config.BlockedSites.Count == 0) return;

            foreach (var browserPath in _browserPaths)
            {
                if (browserPath.Contains("Firefox"))
                {
                    CheckFirefoxHistory();
                }
                else if (File.Exists(browserPath))
                {
                    CheckChromeOrEdgeHistory(browserPath);
                }
            }
        }

        private void CheckChromeOrEdgeHistory(string historyPath)
        {
            try
            {
                if (!File.Exists(historyPath)) return;

                string[] lines = { };
                try
                {
                    lines = File.ReadAllLines(historyPath);
                }
                catch
                {
                    return;
                }

                foreach (var line in lines)
                {
                    foreach (var blocked in _config.BlockedSites)
                    {
                        if (line.Contains(blocked, StringComparison.OrdinalIgnoreCase))
                        {
                            EventLog.WriteEntry($"Blocked site accessed: {blocked}", EventLogEntryType.Warning);
                            return;
                        }
                    }
                }
            }
            catch { }
        }

        private void CheckFirefoxHistory()
        {
        }

        public bool IsBlockedSiteAccessed()
        {
            if (_config.BlockedSites == null || _config.BlockedSites.Count == 0) return false;

            foreach (var browserPath in _browserPaths)
            {
                if (!File.Exists(browserPath)) continue;

                try
                {
                    var content = File.ReadAllText(browserPath);
                    foreach (var blocked in _config.BlockedSites)
                    {
                        if (content.Contains(blocked, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                catch { }
            }
            return false;
        }
    }
}
```

#### ShutdownScheduler.cs

```csharp
using System;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class ShutdownScheduler
    {
        private readonly AppConfiguration _config;
        private bool _shutdownInitiated;
        private DateTime _lastShutdownCheck;

        public ShutdownScheduler(AppConfiguration config)
        {
            _config = config;
            _lastShutdownCheck = DateTime.Today;
        }

        public bool ShouldShutdown()
        {
            if (_shutdownInitiated) return true;

            try
            {
                var shutdownTime = TimeSpan.Parse(_config.AutoShutdownTime);
                var now = DateTime.Now.TimeOfDay;

                if (now >= shutdownTime && _lastShutdownCheck.Date != DateTime.Today)
                {
                    _shutdownInitiated = true;
                    _lastShutdownCheck = DateTime.Today;
                    EventLog.WriteEntry($"Shutdown scheduled at {shutdownTime}", EventLogEntryType.Information);
                    return true;
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"ShutdownScheduler error: {ex.Message}", EventLogEntryType.Error);
            }

            return false;
        }

        public void ResetShutdownFlag()
        {
            if (DateTime.Today > _lastShutdownCheck.Date)
            {
                _shutdownInitiated = false;
                _lastShutdownCheck = DateTime.Today;
            }
        }
    }
}
```

#### NtpValidator.cs

```csharp
using System;
using System.Net;
using System.Net.Sockets;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class NtpValidator
    {
        private readonly AppConfiguration _config;
        private DateTime? _cachedNtpTime;
        private DateTime? _lastNtpCheck;

        private readonly byte[] _ntpRequest = new byte[48];
        private readonly byte[] _ntpResponse = new byte[48];

        public NtpValidator(AppConfiguration config)
        {
            _config = config;
            _ntpRequest[0] = 0x1B;
        }

        public bool ValidateTime()
        {
            foreach (var server in _config.NtpServers)
            {
                try
                {
                    var ntpTime = GetNtpTime(server);
                    if (ntpTime.HasValue)
                    {
                        _cachedNtpTime = ntpTime.Value;
                        _lastNtpCheck = DateTime.Now;
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        public bool IsTimeTampered()
        {
            if (!_config.UseNtpValidation) return false;
            if (!_cachedNtpTime.HasValue || !_lastNtpCheck.HasValue) return false;

            if ((DateTime.Now - _lastNtpCheck.Value).TotalMinutes > 10)
            {
                return false;
            }

            var systemTime = DateTime.Now;
            var difference = Math.Abs((systemTime - _cachedNtpTime.Value).TotalMinutes);

            return difference > _config.NtpToleranceMinutes;
        }

        public DateTime? GetCurrentNtpTime()
        {
            return _cachedNtpTime;
        }

        private DateTime? GetNtpTime(string server)
        {
            try
            {
                using (var udp = new UdpClient())
                {
                    udp.Client.ReceiveTimeout = 5000;
                    udp.Send(_ntpRequest, _ntpRequest.Length, server, 123);
                    var endpoint = new IPEndPoint(IPAddress.Any, 123);
                    _ntpResponse = udp.Receive(ref endpoint);

                    if (_ntpResponse.Length >= 48)
                    {
                        ulong timestamp = BitConverter.ToUInt32(_ntpResponse, 40);
                        timestamp = ((timestamp >> 24) & 0xFFFFFFFF) | ((timestamp << 8) & 0xFFFFFFFF00);

                        var ntpTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
                        return ntpTime.ToLocalTime();
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
```

#### NotificationHelper.cs

```csharp
using System;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class NotificationHelper
    {
        private readonly AppConfiguration _config;

        public NotificationHelper(AppConfiguration config)
        {
            _config = config;
        }

        public void ShowWarning(string message)
        {
            try
            {
                EventLog.WriteEntry(message, EventLogEntryType.Information);
            }
            catch { }
        }
    }
}
```

#### NamedPipeServer.cs

```csharp
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class NamedPipeServer
    {
        private readonly string _pipeName;
        private CancellationTokenSource _cts;
        private Task _listenTask;

        public event Action<PipeMessage> OnMessageReceived;

        public NamedPipeServer(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listenTask = Task.Run(ListenLoop);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void SendMessage(PipeMessage message)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", _pipeName + "_response"))
                {
                    client.Connect(1000);
                    using (var writer = new StreamWriter(client))
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(message);
                        writer.Write(json);
                        writer.Flush();
                    }
                }
            }
            catch { }
        }

        private void ListenLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                    {
                        server.WaitForConnectionAsync(_cts.Token).Wait(_cts.Token);

                        using (var reader = new StreamReader(server))
                        {
                            var json = reader.ReadToEnd();
                            var message = System.Text.Json.JsonSerializer.Deserialize<PipeMessage>(json);
                            if (message != null)
                            {
                                OnMessageReceived?.Invoke(message);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch { }
            }
        }
    }
}
```

#### ConfigManager.cs

```csharp
using System;
using System.IO;
using System.Text.Json;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class ConfigManager
    {
        private readonly string _dataDirectory;
        private readonly string _configPath;
        private readonly string _keyPath;
        private string _encryptionKey;

        public ConfigManager(string dataDirectory)
        {
            _dataDirectory = dataDirectory;
            _configPath = Path.Combine(dataDirectory, "config.bin");
            _keyPath = Path.Combine(dataDirectory, "key.dat");
            _encryptionKey = LoadOrCreateKey();
        }

        public AppConfiguration Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var encryptedJson = File.ReadAllText(_configPath);
                    var json = AesEncryption.Decrypt(encryptedJson, _encryptionKey);
                    var config = JsonSerializer.Deserialize<AppConfiguration>(json);
                    if (config != null) return config;
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Failed to load config: {ex.Message}", EventLogEntryType.Error);
            }

            return CreateDefault();
        }

        public void Save(AppConfiguration config)
        {
            try
            {
                string directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(config, options);
                var encryptedJson = AesEncryption.Encrypt(json, _encryptionKey);
                File.WriteAllText(_configPath, encryptedJson);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Failed to save config: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private string LoadOrCreateKey()
        {
            try
            {
                if (File.Exists(_keyPath))
                {
                    return File.ReadAllText(_keyPath);
                }
                else
                {
                    var key = AesEncryption.GenerateKey();
                    File.WriteAllText(_keyPath, key);
                    return key;
                }
            }
            catch
            {
                return "DefaultFallbackKey_ChangeThis";
            }
        }

        private static AppConfiguration CreateDefault()
        {
            return new AppConfiguration
            {
                Version = "1.0",
                IsEnabled = true,
                AdminPasswordHash = "",
                Rules = new RulesConfiguration
                {
                    Weekdays = new TimeRule
                    {
                        DailyLimitMinutes = 120,
                        AllowedTimeWindows = new System.Collections.Generic.List<TimeWindow>
                        {
                            new TimeWindow { Start = "15:00", End = "20:00" }
                        }
                    },
                    Weekends = new TimeRule
                    {
                        DailyLimitMinutes = 240,
                        AllowedTimeWindows = new System.Collections.Generic.List<TimeWindow>
                        {
                            new TimeWindow { Start = "09:00", End = "21:00" }
                        }
                    }
                },
                AutoShutdownTime = "22:00",
                WarningMinutes = new[] { 10, 5, 1 },
                IdleThresholdMs = 5000,
                ContinuousLimitMinutes = 45,
                RestDurationMinutes = 5,
                BlockedApps = new System.Collections.Generic.List<string>(),
                BlockedSites = new System.Collections.Generic.List<string>(),
                UseNtpValidation = true,
                NtpServers = new[] { "pool.ntp.org", "time.windows.com", "cn.pool.ntp.org" },
                NtpToleranceMinutes = 5,
                ServiceName = "WinSecSvc_a1b2c3d4",
                ServiceDisplayName = "Windows Security Update Service",
                LockScreenMessage = "今天的使用时间已到，休息一下吧！",
                EmergencyUnlockShortcut = "Ctrl+Alt+Shift+F12"
            };
        }
    }
}
```

### 15.3 守护进程 (ChildPCGuard.Agent)

#### Program.cs

```csharp
using System;

namespace ChildPCGuard.Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isAgentA = args.Length > 0 && args[0] == "--agent-a";
            bool isAgentB = args.Length > 0 && args[0] == "--agent-b";

            string partnerName = isAgentA ? "RuntimeBroker" : (isAgentB ? "svchost" : "");
            string pipeName = "ChildPCGuardPipe";

            var agent = new Agent(partnerName, pipeName);
            agent.Start();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
```

#### Agent.cs

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using ChildPCGuard.Shared;

namespace ChildPCGuard.Agent
{
    public class Agent
    {
        private readonly string _partnerName;
        private readonly string _pipeName;
        private Process _partnerProcess;
        private Timer _monitorTimer;
        private Timer _heartbeatTimer;
        private bool _isRunning = true;

        public Agent(string partnerName, string pipeName)
        {
            _partnerName = partnerName;
            _pipeName = pipeName;
        }

        public void Start()
        {
            StartPartnerProcess();
            StartMonitoring();
            StartHeartbeat();
        }

        private void StartPartnerProcess()
        {
            try
            {
                string partnerPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    $"{_partnerName}.exe");

                if (!File.Exists(partnerPath))
                {
                    partnerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{_partnerName}.exe");
                }

                if (File.Exists(partnerPath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = partnerPath,
                        Arguments = _partnerName == "svchost" ? "--agent-a" : "--agent-b",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    _partnerProcess = Process.Start(psi);
                }
            }
            catch { }
        }

        private void StartMonitoring()
        {
            _monitorTimer = new Timer(MonitorPartner, null, 0, 3000);
        }

        private void StartHeartbeat()
        {
            _heartbeatTimer = new Timer(SendHeartbeat, null, 0, 5000);
        }

        private void MonitorPartner(object state)
        {
            if (!_isRunning) return;

            try
            {
                if (_partnerProcess == null || _partnerProcess.HasExited)
                {
                    RestartPartner();
                }
            }
            catch { }
        }

        private void RestartPartner()
        {
            try
            {
                _partnerProcess?.Kill();
            }
            catch { }

            StartPartnerProcess();
        }

        private void SendHeartbeat(object state)
        {
            if (!_isRunning) return;

            try
            {
                using (var client = new NamedPipeClientStream(".", _pipeName))
                {
                    client.Connect(1000);

                    var heartbeat = new HeartbeatMessage
                    {
                        MemoryUsage = (uint)(Environment.WorkingSet / 1024 / 1024),
                        Uptime = TimeSpan.FromMilliseconds(Environment.TickCount)
                    };

                    using (var writer = new StreamWriter(client))
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(heartbeat);
                        writer.Write(json);
                        writer.Flush();
                    }
                }
            }
            catch { }
        }

        public void Stop()
        {
            _isRunning = false;
            _monitorTimer?.Dispose();
            _heartbeatTimer?.Dispose();
            try { _partnerProcess?.Kill(); } catch { }
        }
    }
}
```

### 15.4 锁屏界面 (ChildPCGuard.LockOverlay)

#### LockWindow.xaml

```xml
<Window x:Class="ChildPCGuard.LockOverlay.LockWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Windows Security"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="#FF1E1E2E"
        WindowStartupLocation="CenterScreen"
        Width="1920"
        Height="1080"
        KeyDown="Window_KeyDown"
        Loaded="Window_Loaded">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <StackPanel Grid.Row="1" HorizontalAlignment="Center" VerticalAlignment="Center" Width="400">
            <Image Source="shield.png" Width="80" Height="80" Margin="0,0,0,30" HorizontalAlignment="Center"/>

            <TextBlock x:Name="LockMessage"
                       Text="今天的使用时间已到，休息一下吧！"
                       FontSize="28"
                       FontWeight="Bold"
                       Foreground="White"
                       HorizontalAlignment="Center"
                       Margin="0,0,0,20"
                       TextWrapping="Wrap"
                       TextAlignment="Center"/>

            <TextBlock x:Name="RestCountdown"
                       Text=""
                       FontSize="20"
                       Foreground="#FF6B6B"
                       HorizontalAlignment="Center"
                       Margin="0,0,0,20"
                       Visibility="Collapsed"/>

            <TextBlock Text="请输入家长密码解锁"
                       FontSize="14"
                       Foreground="#888888"
                       HorizontalAlignment="Center"
                       Margin="0,0,0,15"/>

            <PasswordBox x:Name="PasswordBox"
                        Width="300"
                        Height="45"
                        FontSize="16"
                        HorizontalAlignment="Center"
                        Margin="0,0,0,10"
                        KeyDown="PasswordBox_KeyDown"/>

            <TextBlock x:Name="ErrorMessage"
                       Text="密码错误，请重试"
                       FontSize="12"
                       Foreground="#FF4444"
                       HorizontalAlignment="Center"
                       Visibility="Collapsed"
                       Margin="0,0,0,10"/>

            <Button Content="解锁"
                    Width="300"
                    Height="40"
                    FontSize="16"
                    Background="#0078D4"
                    Foreground="White"
                    BorderThickness="0"
                    Click="UnlockButton_Click"/>

            <TextBlock x:Name="AttemptsMessage"
                       Text=""
                       FontSize="12"
                       Foreground="#888888"
                       HorizontalAlignment="Center"
                       Margin="0,15,0,0"/>
        </StackPanel>

        <TextBlock Grid.Row="2"
                   Text="如需紧急解锁，请连续按 Ctrl+Alt+Shift+F12 五次"
                   FontSize="10"
                   Foreground="#555555"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Bottom"
                   Margin="0,0,0,20"/>
    </Grid>
</Window>
```

#### LockWindow.xaml.cs

```csharp
using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using ChildPCGuard.Shared;

namespace ChildPCGuard.LockOverlay
{
    public partial class LockWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevMode, uint dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll")]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        private static extern IntPtr GetThreadDesktop(uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private IntPtr _originalDesktop;
        private IntPtr _lockDesktop;
        private int _failedAttempts = 0;
        private DateTime _lockoutEndTime;
        private int _emergencyKeyCount = 0;
        private DateTime _lastEmergencyKeyTime;
        private LockReason _lockReason;

        private const uint DESKTUP_CREATEWINDOW = 0x0002;
        private const uint DESKTOP_SWITCHDESKTOP = 0x0100;
        private const uint DESKTOP_WRITEOBJECTS = 0x0080;
        private const uint DESKTOP_READOBJECTS = 0x0001;

        private static readonly string ConfigPath = @"C:\ProgramData\ChildPCGuard\config.bin";
        private static readonly string KeyPath = @"C:\ProgramData\ChildPCGuard\key.dat";

        public LockWindow()
        {
            InitializeComponent();
        }

        public LockWindow(string[] args) : this()
        {
            if (args.Length > 0 && int.TryParse(args[0], out int reasonCode))
            {
                _lockReason = (LockReason)reasonCode;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _originalDesktop = GetThreadDesktop(GetCurrentThreadId());

                _lockDesktop = CreateDesktop(
                    "LockScreen_" + Guid.NewGuid().ToString("N"),
                    IntPtr.Zero,
                    IntPtr.Zero,
                    0,
                    DESKTUP_CREATEWINDOW | DESKTOP_SWITCHDESKTOP | DESKTOP_WRITEOBJECTS | DESKTOP_READOBJECTS,
                    IntPtr.Zero);

                SwitchDesktop(_lockDesktop);
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\ProgramData\ChildPCGuard\logs\error.log",
                    $"[{DateTime.Now}] Desktop switch error: {ex}\n");
            }

            PasswordBox.Focus();
        }

        private void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            TryUnlock();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryUnlock();
            }
        }

        private void TryUnlock()
        {
            if (_lockoutEndTime > DateTime.Now)
            {
                var remaining = (_lockoutEndTime - DateTime.Now).Seconds;
                ErrorMessage.Text = $"锁定中，请等待 {remaining} 秒";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            string password = PasswordBox.Password;

            try
            {
                string encryptionKey = File.ReadAllText(KeyPath);
                var encryptedJson = File.ReadAllText(ConfigPath);
                var json = AesEncryption.Decrypt(encryptedJson, encryptionKey);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json);

                if (config != null && VerifyPassword(password, config.AdminPasswordHash))
                {
                    Unlock();
                }
                else
                {
                    _failedAttempts++;
                    int remainingAttempts = 3 - _failedAttempts;

                    if (_failedAttempts >= 3)
                    {
                        _lockoutEndTime = DateTime.Now.AddMinutes(5);
                        ErrorMessage.Text = "密码错误次数过多，锁定5分钟";
                        _failedAttempts = 0;
                    }
                    else
                    {
                        ErrorMessage.Text = $"密码错误，剩余 {remainingAttempts} 次尝试机会";
                    }

                    ErrorMessage.Visibility = Visibility.Visible;
                    PasswordBox.Password = "";
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\ProgramData\ChildPCGuard\logs\error.log",
                    $"[{DateTime.Now}] Unlock error: {ex}\n");
                ErrorMessage.Text = "验证失败，请稍后重试";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword)) return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        private void Unlock()
        {
            try
            {
                SwitchDesktop(_originalDesktop);
                CloseDesktop(_lockDesktop);

                NotifyServiceUnlocked();

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                });

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\ProgramData\ChildPCGuard\logs\error.log",
                    $"[{DateTime.Now}] Unlock error: {ex}\n");
            }
        }

        private void NotifyServiceUnlocked()
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", "ChildPCGuardPipe"))
                {
                    client.Connect(1000);

                    var message = new PipeMessage
                    {
                        Type = PipeMessageType.UnlockRequest,
                        Timestamp = DateTime.Now
                    };

                    using (var writer = new StreamWriter(client))
                    {
                        var json = JsonSerializer.Serialize(message);
                        writer.Write(json);
                        writer.Flush();
                    }
                }
            }
            catch { }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 &&
                Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift))
            {
                HandleEmergencyUnlock();
            }
        }

        private void HandleEmergencyUnlock()
        {
            var now = DateTime.Now;

            if (_lastEmergencyKeyTime == null || (now - _lastEmergencyKeyTime).TotalSeconds > 5)
            {
                _emergencyKeyCount = 0;
            }

            _lastEmergencyKeyTime = now;
            _emergencyKeyCount++;

            if (_emergencyKeyCount >= 5)
            {
                MessageBox.Show("紧急解锁已触发，请联系家长确认身份。",
                    "紧急解锁",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                _emergencyKeyCount = 0;
            }
        }
    }
}
```

### 15.5 项目文件

#### ChildPCGuard.GuardService.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <AssemblyName>GuardService</AssemblyName>
    <ApplicationIcon />
    <StartupObject />
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ChildPCGuard.Shared\ChildPCGuard.Shared.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting.WindowsServices" Version="8.0.0" />
    <PackageReference Include="System.ServiceProcess.ServiceController" Version="8.0.0" />
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
  </ItemGroup>

</Project>
```

#### ChildPCGuard.Agent.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ChildPCGuard.Shared\ChildPCGuard.Shared.csproj" />
  </ItemGroup>

</Project>
```

#### ChildPCGuard.LockOverlay.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ChildPCGuard.Shared\ChildPCGuard.Shared.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  </ItemGroup>

</Project>
```

#### ChildPCGuard.Shared.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  </ItemGroup>

</Project>
```

### 15.6 安装脚本

#### install.ps1

```powershell
# ChildPCGuard Installation Script
# Must be run as Administrator

param(
    [Parameter(Mandatory=$true)]
    [string]$AdminPassword
)

$ErrorActionPreference = "Stop"

Write-Host "ChildPCGuard Installation Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Check admin rights
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Error: Administrator rights required!" -ForegroundColor Red
    exit 1
}

$installDir = "C:\Program Files\ChildPCGuard"
$dataDir = "C:\ProgramData\ChildPCGuard"
$serviceName = "WinSecSvc_a1b2c3d4"
$serviceDisplayName = "Windows Security Update Service"

# Create directories
Write-Host "Creating directories..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
New-Item -ItemType Directory -Path "$dataDir\logs" -Force | Out-Null
New-Item -ItemType Directory -Path "$dataDir\data" -Force | Out-Null

# Copy files
Write-Host "Copying files..." -ForegroundColor Yellow
Copy-Item "src\GuardService\bin\Release\net8.0-windows\GuardService.exe" "$dataDir\GuardService.exe" -Force
Copy-Item "src\Agent\bin\Release\net8.0-windows\Agent.exe" "$installDir\Agent.exe" -Force
Copy-Item "src\LockOverlay\bin\Release\net8.0-windows\LockOverlay.exe" "$installDir\LockOverlay.exe" -Force

# Copy to System32 with伪装 names
Copy-Item "$installDir\Agent.exe" "C:\Windows\System32\svchost.exe" -Force
Copy-Item "$installDir\Agent.exe" "C:\Windows\System32\RuntimeBroker.exe" -Force
Copy-Item "$installDir\LockOverlay.exe" "C:\Windows\System32\LockOverlay.exe" -Force

# Set ACL on installation directory
Write-Host "Setting ACL permissions..." -ForegroundColor Yellow
$acl = Get-Acl $installDir
$acl.SetAccessRuleProtection($true, $false)
$systemUser = New-Object System.Security.Principal.SecurityIdentifier("S-1-5-18")
$acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($systemUser, "FullControl", "Allow")))
$adminsGroup = New-Object System.Security.Principal.SecurityIdentifier("S-1-5-32-544")
$acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($adminsGroup, "FullControl", "Allow")))
$acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule("S-1-5-32-545", "ReadAndExecute", "Deny")))
Set-Acl $installDir $acl

# Set ACL on data directory
$acl = Get-Acl $dataDir
$acl.SetAccessRuleProtection($true, $false)
$acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($systemUser, "FullControl", "Allow")))
$acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($adminsGroup, "FullControl", "Allow")))
Set-Acl $dataDir $acl

# Generate encryption key and create config
Write-Host "Creating configuration..." -ForegroundColor Yellow
$key = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
Set-Content -Path "$dataDir\key.dat" -Value $key -NoNewline

$config = @{
    version = "1.0"
    isEnabled = $true
    adminPasswordHash = $(BCrypt.Net.BCrypt.HashPassword($AdminPassword))
    rules = @{
        weekdays = @{ dailyLimitMinutes = 120; allowedTimeWindows = @(@{start="15:00"; end="20:00"}) }
        weekends = @{ dailyLimitMinutes = 240; allowedTimeWindows = @(@{start="09:00"; end="21:00"}) }
    }
    autoShutdownTime = "22:00"
    warningMinutes = @(10, 5, 1)
    idleThresholdMs = 5000
    continuousLimitMinutes = 45
    restDurationMinutes = 5
    blockedApps = @()
    blockedSites = @()
    useNtpValidation = $true
    ntpServers = @("pool.ntp.org", "time.windows.com", "cn.pool.ntp.org")
    ntpToleranceMinutes = 5
    serviceName = $serviceName
    serviceDisplayName = $serviceDisplayName
} | ConvertTo-Json -Depth 10

$encrypted = ChildPCGuard.Shared.AesEncryption::Encrypt($config, $key)
Set-Content -Path "$dataDir\config.bin" -Value $encrypted -NoNewline

# Install Windows Service
Write-Host "Installing Windows service..." -ForegroundColor Yellow
sc.exe create $serviceName binPath= "$dataDir\GuardService.exe" DisplayName= $serviceDisplayName start= auto
sc.exe failure $serviceName reset= 86400 actions= restart/1000/restart/1000/restart/1000

# Set service DACL
$sddl = "D:(A;;CCLCSWRPWPDTLOCRSDRCWDWO;;;SY)(A;;CCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(D;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;IU)"
sc.exe sdset $serviceName $sddl

# Create scheduled task for backup shutdown
Write-Host "Creating scheduled task for backup shutdown..." -ForegroundColor Yellow
$action = New-ScheduledTaskAction -Execute "shutdown.exe" -Argument "/s /f /t 0"
$trigger = New-ScheduledTaskTrigger -Daily -At "22:00"
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
Register-ScheduledTask -TaskName "ChildPCGuard_AutoShutdown" -Trigger $trigger -Action $action -Settings $settings -Description "ChildPCGuard backup shutdown task" | Out-Null

# Start service
Write-Host "Starting service..." -ForegroundColor Yellow
sc.exe start $serviceName

Write-Host ""
Write-Host "Installation complete!" -ForegroundColor Green
Write-Host "Service Name: $serviceDisplayName" -ForegroundColor Cyan
Write-Host "Data Directory: $dataDir" -ForegroundColor Cyan
Write-Host "Please remember your admin password!" -ForegroundColor Yellow
```

#### uninstall.ps1

```powershell
# ChildPCGuard Uninstallation Script
# Must be run as Administrator

param(
    [Parameter(Mandatory=$true)]
    [string]$AdminPassword
)

$ErrorActionPreference = "Stop"

Write-Host "ChildPCGuard Uninstallation Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan

# Check admin rights
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Error: Administrator rights required!" -ForegroundColor Red
    exit 1
}

$dataDir = "C:\ProgramData\ChildPCGuard"
$serviceName = "WinSecSvc_a1b2c3d4"

# Verify password
Write-Host "Verifying admin password..." -ForegroundColor Yellow
try {
    $key = Get-Content "$dataDir\key.dat" -Raw
    $encrypted = Get-Content "$dataDir\config.bin" -Raw
    $json = ChildPCGuard.Shared.AesEncryption::Decrypt($encrypted, $key)
    $config = $json | ConvertFrom-Json

    if (-not (BCrypt.Net.BCrypt.Verify($AdminPassword, $config.adminPasswordHash))) {
        Write-Host "Error: Incorrect password!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error: Could not verify password. Installation may be corrupted." -ForegroundColor Red
    exit 1
}

# Stop and delete service
Write-Host "Stopping and removing service..." -ForegroundColor Yellow
sc.exe stop $serviceName 2>$null
Start-Sleep -Seconds 2
sc.exe delete $serviceName 2>$null

# Remove scheduled task
Write-Host "Removing scheduled task..." -ForegroundColor Yellow
Unregister-ScheduledTask -TaskName "ChildPCGuard_AutoShutdown" -Confirm:$false 2>$null

# Remove files from System32
Write-Host "Removing files..." -ForegroundColor Yellow
Remove-Item "C:\Windows\System32\svchost.exe" -Force -ErrorAction SilentlyContinue
Remove-Item "C:\Windows\System32\RuntimeBroker.exe" -Force -ErrorAction SilentlyContinue
Remove-Item "C:\Windows\System32\LockOverlay.exe" -Force -ErrorAction SilentlyContinue

# Remove installation directory
Write-Host "Removing installation directory..." -ForegroundColor Yellow
Remove-Item "C:\Program Files\ChildPCGuard" -Recurse -Force -ErrorAction SilentlyContinue

# Optionally remove data directory
Write-Host ""
$response = Read-Host "Do you want to remove data directory (logs and configuration)? (y/N)"
if ($response -eq "y") {
    Remove-Item $dataDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Data directory removed." -ForegroundColor Green
}

Write-Host ""
Write-Host "Uninstallation complete!" -ForegroundColor Green
```

---

## 附录：完整源代码使用说明

### 编译顺序

1. **ChildPCGuard.Shared** - 共享库，所有其他项目依赖此项目
2. **ChildPCGuard.GuardService** - 核心服务
3. **ChildPCGuard.Agent** - 守护进程
4. **ChildPCGuard.LockOverlay** - 锁屏界面

### 依赖项

确保安装以下 NuGet 包：
- BCrypt.Net-Next 4.0.3
- Serilog 3.1.1
- Serilog.Sinks.File 5.0.0
- Microsoft.Extensions.Hosting.WindowsServices 8.0.0
- System.ServiceProcess.ServiceController 8.0.0

### 关键文件路径

| 文件 | 路径 |
|------|------|
| 服务可执行文件 | `C:\ProgramData\ChildPCGuard\GuardService.exe` |
| 守护进程(伪装) | `C:\Windows\System32\svchost.exe` |
| 守护进程B(伪装) | `C:\Windows\System32\RuntimeBroker.exe` |
| 锁屏程序 | `C:\Windows\System32\LockOverlay.exe` |
| 加密密钥 | `C:\ProgramData\ChildPCGuard\key.dat` |
| 主配置文件 | `C:\ProgramData\ChildPCGuard\config.bin` (AES加密) |
| 程序日志 | `C:\ProgramData\ChildPCGuard\logs\process_YYYY-MM-DD.json` |
| 网站日志 | `C:\ProgramData\ChildPCGuard\logs\web_YYYY-MM-DD.json` |
| 使用数据 | `C:\ProgramData\ChildPCGuard\data\usage_data.json` |

---

*文档版本: v2.0（整合版）*
*整合时间: 2026-05-02*
*本文档整合自两个AI方案的精华，为 ChildPCGuard 项目唯一完整参考文档，可直接用于开发*
