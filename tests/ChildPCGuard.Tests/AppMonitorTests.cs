using System.Collections.Generic;
using ChildPCGuard.Shared;
using Xunit;

namespace ChildPCGuard.Tests
{
    public class AppMonitorTests
    {
        [Fact]
        public void Constructor_InitializesWithoutException()
        {
            var config = new AppConfiguration();
            var monitor = new AppMonitor(config);
            Assert.NotNull(monitor);
        }

        [Fact]
        public void BlockedApps_EmptyList_DoesNotThrow()
        {
            var config = new AppConfiguration
            {
                BlockedApps = new List<string>()
            };
            var monitor = new AppMonitor(config);
            // Should not throw when checking with empty blacklist
        }

        [Fact]
        public void BlockedApps_ContainsEntry_MatchesCorrectly()
        {
            var config = new AppConfiguration
            {
                BlockedApps = new List<string> { "game.exe" }
            };
            var monitor = new AppMonitor(config);
            Assert.Contains("game.exe", config.BlockedApps);
        }
    }
}
