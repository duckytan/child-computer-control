using System;
using ChildPCGuard.Shared;
using Xunit;

namespace ChildPCGuard.Tests
{
    public class TimeTrackerTests
    {
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
        public void GetState_InitialState_ReturnsNormal()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            var state = tracker.GetState();
            Assert.Equal(UsageState.Normal, state.State);
        }

        [Fact]
        public void RecordActivity_SetsStateToUsing()
        {
            var config = CreateDefaultConfig();
            var tracker = new TimeTracker(config);
            tracker.RecordActivity(DateTime.Now);
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
            tracker.RecordActivity(DateTime.Now);
            tracker.ResetDaily();
            var state = tracker.GetState();
            Assert.Equal(TimeSpan.Zero, state.UsedTime);
        }
    }
}
