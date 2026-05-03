using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class TimeTracker
    {
        private readonly AppConfiguration _config;
        private DailyUsageData _todayData;
        private DateTime _lastCheckTime;

        private static readonly string DataPath = Path.Combine(@"C:\ProgramData\ChildPCGuard\data", "usage_data.json");

        public TimeTracker(AppConfiguration config)
        {
            _config = config;
            _lastCheckTime = DateTime.Now;
            ResetIfNewDay();
            LoadDailyData();
        }

        public void Update()
        {
            NativeAPI.LASTINPUTINFO lastInput = new NativeAPI.LASTINPUTINFO();
            lastInput.cbSize = (uint)Marshal.SizeOf(lastInput);

            if (NativeAPI.GetLastInputInfo(ref lastInput))
            {
                uint currentTick = (uint)Environment.TickCount;
                uint idleMs = currentTick - lastInput.dwTime;

                if (_lastCheckTime != DateTime.MinValue)
                {
                    var elapsed = DateTime.Now - _lastCheckTime;

                    if (idleMs < _config.IdleThresholdMs)
                    {
                        _todayData.TotalUsedTime += elapsed;
                        _todayData.ContinuousUsedTime += elapsed;

                        if (_todayData.CurrentState == UsageState.Resting)
                        {
                            _todayData.CurrentState = UsageState.Using;
                        }
                    }
                }

                _todayData.LastInputTime = DateTime.Now;
            }

            _lastCheckTime = DateTime.Now;
            SaveDailyData();
        }

        public (UsageState State, TimeSpan UsedTime, TimeSpan ContinuousTime, TimeSpan RestRemainingTime, int ExtraMinutes, TimeSpan RemainingTime) GetState()
        {
            ResetIfNewDay();

            int limit = IsWeekend() ? _config.Rules.Weekends.DailyLimitMinutes : _config.Rules.Weekdays.DailyLimitMinutes;
            var usedTime = _todayData.TotalUsedTime + TimeSpan.FromMinutes(_todayData.ExtraMinutesToday);
            var remainingTime = TimeSpan.FromMinutes(limit) - usedTime;

            return (
                _todayData.CurrentState,
                usedTime,
                _todayData.ContinuousUsedTime,
                _todayData.CurrentState == UsageState.Resting && _todayData.RestEndTime.HasValue
                    ? _todayData.RestEndTime.Value - DateTime.Now
                    : TimeSpan.Zero,
                _todayData.ExtraMinutesToday,
                remainingTime
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
                if (System.IO.File.Exists(DataPath))
                {
                    var json = System.IO.File.ReadAllText(DataPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<DailyUsageData>(json, options);
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
                string directory = Path.GetDirectoryName(DataPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
                var json = JsonSerializer.Serialize(_todayData, options);
                System.IO.File.WriteAllText(DataPath, json);
            }
            catch { }
        }
    }
}
