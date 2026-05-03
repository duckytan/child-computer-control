using System;
using System.Collections.Generic;

namespace ChildPCGuard.Shared
{
    public class AppConfiguration
    {
        public string Version { get; set; } = "1.0";
        public bool IsEnabled { get; set; } = true;
        public string AdminPasswordHash { get; set; } = string.Empty;
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
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
    }

    public class DailyUsageData
    {
        public string Date { get; set; } = string.Empty;
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
        public string Date { get; set; } = string.Empty;
        public List<ProcessUsageRecord> Records { get; set; } = new List<ProcessUsageRecord>();
    }

    public class ProcessUsageRecord
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessPath { get; set; } = string.Empty;
        public int Duration { get; set; }
    }

    public class WebUsageLog
    {
        public string Date { get; set; } = string.Empty;
        public List<WebUsageRecord> Records { get; set; } = new List<WebUsageRecord>();
    }

    public class WebUsageRecord
    {
        public DateTime Timestamp { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
