using System;
using System.Diagnostics;
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
        private FileSystemWatcher _configWatcher;

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
        private readonly object _configLock = new object();

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

                _configWatcher = new FileSystemWatcher(DataDirectory, "config.json");
                _configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                _configWatcher.Changed += OnConfigChanged;
                _configWatcher.EnableRaisingEvents = true;

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
            _configWatcher?.Dispose();
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
                lock (_configLock)
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
                lock (_configLock)
                {
                    var config = _configManager.Load();
                    if (_shutdownScheduler.ShouldShutdown())
                    {
                        _notificationHelper.ShowWarning("电脑将在 60 秒后自动关机");

                        Task.Delay(60000).ContinueWith(t =>
                        {
                            try
                            {
                                TriggerShutdown();
                            }
                            catch (Exception ex)
                            {
                                EventLog.WriteEntry($"Shutdown execution error: {ex.Message}", EventLogEntryType.Error);
                            }
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);
                    }
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
                lock (_configLock)
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
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"NTP check error: {ex.Message}", EventLogEntryType.Warning);
            }
        }

        private void OnPipeMessageReceived(PipeMessage message)
        {
            try
            {
                lock (_configLock)
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
                            if (int.TryParse(message.Payload, out int minutes) && minutes > 0)
                            {
                                _timeTracker.AddExtraTime(minutes);
                            }
                            else
                            {
                                EventLog.WriteEntry($"Invalid AddTime payload: {message.Payload}", EventLogEntryType.Warning);
                            }
                            break;
                        case PipeMessageType.LockNow:
                            TriggerLockScreen(LockReason.ManualLock);
                            break;
                        case PipeMessageType.ShutdownNow:
                            TriggerShutdown();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Pipe message error: {ex.Message}", EventLogEntryType.Warning);
            }
        }

        private void HandleUnlockRequest(PipeMessage message)
        {
            var config = _configManager.Load();
            if (string.IsNullOrEmpty(config.AdminPasswordHash)) return;

            string inputHash = AesEncryption.ComputeHash(message.Payload);
            if (inputHash == config.AdminPasswordHash)
            {
                _timeTracker.ResetDaily();
                EventLog.WriteEntry("Service unlocked by password.", EventLogEntryType.Information);
            }
        }

        private void OnAgentDead(string agentName)
        {
            EventLog.WriteEntry($"Agent {agentName} died, restarting...", EventLogEntryType.Warning);
            _processGuardian.RestartAgent(agentName);
        }

        private void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                Thread.Sleep(500);

                _configManager.Reload();

                var config = _configManager.Load();

                lock (_configLock)
                {
                    _processGuardian = new ProcessGuardian(config);
                    _appMonitor = new AppMonitor(config);
                    _webMonitor = new WebMonitor(config);
                    _shutdownScheduler = new ShutdownScheduler(config);
                    _ntpValidator = new NtpValidator(config);
                    _notificationHelper = new NotificationHelper(config);
                }

                EventLog.WriteEntry("Configuration reloaded successfully.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Config reload error: {ex.Message}", EventLogEntryType.Warning);
            }
        }

        private StatusMessage CreateStatusMessage()
        {
            var stateInfo = _timeTracker.GetState();
            var config = _configManager.Load();

            return new StatusMessage
            {
                CurrentState = stateInfo.State,
                UsedTimeToday = stateInfo.UsedTime,
                RemainingTime = stateInfo.RemainingTime,
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
