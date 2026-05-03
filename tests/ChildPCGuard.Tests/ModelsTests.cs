using System;
using ChildPCGuard.Shared;
using Xunit;

namespace ChildPCGuard.Tests
{
    public class ModelsTests
    {
        [Fact]
        public void AppConfiguration_DefaultConstructor_HasCorrectDefaults()
        {
            var config = new AppConfiguration();

            Assert.Equal("1.0", config.Version);
            Assert.True(config.IsEnabled);
            Assert.Equal("22:00", config.AutoShutdownTime);
            Assert.Equal(45, config.ContinuousLimitMinutes);
            Assert.Equal(5, config.RestDurationMinutes);
            Assert.Equal(5000, config.IdleThresholdMs);
            Assert.True(config.UseNtpValidation);
            Assert.Equal("WinSecSvc_a1b2c3d4", config.ServiceName);
            Assert.Equal("Windows Security Update Service", config.ServiceDisplayName);
            Assert.NotNull(config.Rules);
            Assert.NotNull(config.BlockedApps);
            Assert.NotNull(config.BlockedSites);
            Assert.Empty(config.BlockedApps);
            Assert.Empty(config.BlockedSites);
        }

        [Fact]
        public void UsageState_Using_HasValueZero()
        {
            Assert.Equal(0, (int)UsageState.Using);
        }

        [Fact]
        public void UsageState_Resting_HasValueOne()
        {
            Assert.Equal(1, (int)UsageState.Resting);
        }

        [Fact]
        public void UsageState_Locked_HasValueTwo()
        {
            Assert.Equal(2, (int)UsageState.Locked);
        }

        [Fact]
        public void UsageState_Shutdown_HasValueThree()
        {
            Assert.Equal(3, (int)UsageState.Shutdown);
        }

        [Fact]
        public void LockReason_EnumHasNineValues()
        {
            var values = Enum.GetValues(typeof(LockReason));
            Assert.Equal(9, values.Length);
        }

        [Fact]
        public void LockReason_DailyLimitReached_HasValueOne()
        {
            Assert.Equal(1, (int)LockReason.DailyLimitReached);
        }

        [Fact]
        public void LockReason_ContinuousLimit_HasValueTwo()
        {
            Assert.Equal(2, (int)LockReason.ContinuousLimit);
        }

        [Fact]
        public void DailyUsageData_DefaultConstructor_HasCorrectDefaults()
        {
            var data = new DailyUsageData();

            Assert.Equal(TimeSpan.Zero, data.TotalUsedTime);
            Assert.Equal(TimeSpan.Zero, data.ContinuousUsedTime);
            Assert.Equal(0, data.ExtraMinutesToday);
            Assert.Equal(UsageState.Normal, data.CurrentState);
        }
    }
}
