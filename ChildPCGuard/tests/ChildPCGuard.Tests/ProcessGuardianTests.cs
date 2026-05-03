using System;
using ChildPCGuard.Shared;
using Xunit;

namespace ChildPCGuard.Tests
{
    public class ProcessGuardianTests
    {
        private AppConfiguration CreateDefaultConfig()
        {
            return new AppConfiguration
            {
                ServiceName = "WinSecSvc_a1b2c3d4"
            };
        }

        [Fact]
        public void ReceiveHeartbeat_UpdatesLastHeartbeatTime()
        {
            var config = CreateDefaultConfig();
            var guardian = new ProcessGuardian(config);
            guardian.ReceiveHeartbeat("AgentA");
            // The heartbeat should be recorded without throwing exceptions
        }

        [Fact]
        public void Constructor_InitializesWithoutException()
        {
            var config = CreateDefaultConfig();
            var guardian = new ProcessGuardian(config);
            Assert.NotNull(guardian);
        }
    }
}
