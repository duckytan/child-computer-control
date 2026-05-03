using System;
using System.Diagnostics;

namespace ChildPCGuard.Shared
{
    [Serializable]
    public class PipeMessage
    {
        public PipeMessageType Type { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Payload { get; set; } = string.Empty;
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

    [Serializable]
    public class LogRecord
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessPath { get; set; } = string.Empty;
        public int Duration { get; set; }
    }

    [Serializable]
    public class WebLogRecord
    {
        public DateTime Timestamp { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

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

    public enum UsageState
    {
        Using = 0,
        Resting = 1,
        Locked = 2,
        Shutdown = 3,
        Paused = 4,
        Normal = 10
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
