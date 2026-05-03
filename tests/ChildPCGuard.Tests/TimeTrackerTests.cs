using System;
using System.IO;
using ChildPCGuard.Shared;
using Xunit;

namespace ChildPCGuard.Tests
{
    public class TimeTrackerTests : IDisposable
    {
        private readonly string _testDataPath;
        private readonly string _originalDataPath;

        public TimeTrackerTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), "ChildPCGuard_test_data");
            Directory.CreateDirectory(_testDataPath);

            var type = typeof(TimeTracker);
            var field = type.GetField("DataPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            _originalDataPath = (string)field.GetValue(null);
            field.SetValue(null, Path.Combine(_testDataPath, "usage_data.json"));
        }

        public void Dispose()
        {
            var type = typeof(TimeTracker);
            var field = type.GetField("DataPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            field.SetValue(null, _originalDataPath);

            if (Directory.Exists(_testDataPath))
            {
                Directory.Delete(_testDataPath, true);
            }
        }

        private AppConfiguration CreateDefaultConfig()
        {
            return new AppConfiguration
            {
                ContinuousLimitMinutes = 45,
                RestDurationMinutes = 5,
                IdleThresholdMs = 5000,
                Rules = new RulesConfiguration
                {
                    Weekdays = new TimeRule
                    {
                        DailyLimitMinutes = 120,
                        AllowedTimeWindows = new System.Collections.Generic.List<TimeWindow>
                        {
                            new TimeWindow { Start = "00:00", End = "23:59" }
                        }
                    },
                    Weekends = new TimeRule
                    {
                        DailyLimitMinutes = 240,
                        AllowedTimeWindows = new System.Collections.Generic.List<TimeWindow>
                        {
                            new TimeWindow { Start = "00:00", End = "23:59" }
                        }
                    }
                }
            };
        }

        [Fact]
        public void Constructor_InitializesWithUsingState()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            var state = tracker.GetState();
            Assert.Equal(UsageState.Using, state.State);
        }

        [Fact]
        public void StartRest_SetsStateToResting()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            tracker.StartRest();
            var state = tracker.GetState();
            Assert.Equal(UsageState.Resting, state.State);
        }

        [Fact]
        public void EndRest_SetsStateToUsing()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            tracker.StartRest();
            tracker.EndRest();
            var state = tracker.GetState();
            Assert.Equal(UsageState.Using, state.State);
        }

        [Fact]
        public void EndRest_ResetsContinuousTime()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            tracker.StartRest();
            tracker.EndRest();
            var state = tracker.GetState();
            Assert.Equal(TimeSpan.Zero, state.ContinuousTime);
        }

        [Fact]
        public void AddExtraTime_IncreasesExtraMinutes()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            tracker.AddExtraTime(30);
            var state = tracker.GetState();
            Assert.Equal(30, state.ExtraMinutes);
        }

        [Fact]
        public void ResetDaily_ResetsUsedTime()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            tracker.ResetDaily();
            var state = tracker.GetState();
            Assert.Equal(TimeSpan.Zero, state.UsedTime);
        }

        [Fact]
        public void GetState_CalculatesRemainingTime()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            var state = tracker.GetState();
            Assert.Equal(120, state.RemainingTime.TotalMinutes, 1);
        }
    }
}
