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
        private string? _currentForegroundProcess;
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
                System.Diagnostics.EventLog.WriteEntry("ChildPCGuard", $"AppMonitor error: {ex.Message}", System.Diagnostics.EventLogEntryType.Warning);
            }
        }

        private Process? GetForegroundWindowProcess()
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
                    System.Diagnostics.EventLog.WriteEntry("ChildPCGuard", $"Blocked app detected: {processName}", System.Diagnostics.EventLogEntryType.Warning);
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

        private void RecordProcessUsage(string? processName, DateTime startTime)
        {
            if (string.IsNullOrEmpty(processName) || startTime == DateTime.MinValue) return;

            try
            {
                var duration = (int)(DateTime.Now - startTime).TotalSeconds;
                if (duration < 5) return;

                string logPath = Path.Combine(@"C:\ProgramData\ChildPCGuard\logs",
                    $"process_{DateTime.Today:yyyy-MM-dd}.json");

                List<ProcessUsageRecord> existingRecords = new List<ProcessUsageRecord>();
                if (File.Exists(logPath))
                {
                    var json = File.ReadAllText(logPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var logs = JsonSerializer.Deserialize<List<ProcessUsageLog>>(json, options);
                    if (logs != null && logs.Count > 0)
                    {
                        existingRecords = logs[0].Records;
                    }
                }

                existingRecords.Add(new ProcessUsageRecord
                {
                    Timestamp = startTime,
                    ProcessName = processName,
                    ProcessPath = GetProcessPath(processName),
                    Duration = duration
                });

                var log = new ProcessUsageLog
                {
                    Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    Records = existingRecords
                };

                var options2 = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
                File.WriteAllText(logPath, JsonSerializer.Serialize(new[] { log }, options2));
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("ChildPCGuard", $"Failed to record process usage: {ex.Message}", System.Diagnostics.EventLogEntryType.Warning);
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
            return string.Empty;
        }
    }
}
