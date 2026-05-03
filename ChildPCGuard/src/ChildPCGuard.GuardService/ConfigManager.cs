using System;
using System.IO;
using System.Text.Json;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class ConfigManager
    {
        private readonly string _configPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private AppConfiguration? _cachedConfig;
        private readonly object _lock = new object();

        public ConfigManager(string dataDirectory)
        {
            _configPath = Path.Combine(dataDirectory, "config.json");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public AppConfiguration Load()
        {
            lock (_lock)
            {
                if (_cachedConfig != null)
                    return _cachedConfig;

                try
                {
                    if (File.Exists(_configPath))
                    {
                        var json = File.ReadAllText(_configPath);
                        _cachedConfig = JsonSerializer.Deserialize<AppConfiguration>(json, _jsonOptions);
                    }
                }
                catch
                {
                }

                if (_cachedConfig == null)
                {
                    _cachedConfig = CreateDefaultConfig();
                    Save(_cachedConfig);
                }

                return _cachedConfig;
            }
        }

        public void Save(AppConfiguration config)
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(_configPath, json);
                _cachedConfig = config;
            }
        }

        private AppConfiguration CreateDefaultConfig()
        {
            return new AppConfiguration
            {
                Version = "1.0",
                IsEnabled = true,
                AdminPasswordHash = string.Empty,
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
