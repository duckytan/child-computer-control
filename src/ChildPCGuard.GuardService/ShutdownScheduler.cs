using System;

namespace ChildPCGuard.GuardService
{
    public class ShutdownScheduler
    {
        private readonly AppConfiguration _config;
        private bool _shutdownInitiated;

        public ShutdownScheduler(AppConfiguration config)
        {
            _config = config;
        }

        public bool ShouldShutdown()
        {
            if (_shutdownInitiated) return false;

            try
            {
                var shutdownTime = TimeSpan.Parse(_config.AutoShutdownTime);
                var now = DateTime.Now.TimeOfDay;

                if (now >= shutdownTime && now < shutdownTime.Add(TimeSpan.FromMinutes(1)))
                {
                    _shutdownInitiated = true;
                    return true;
                }
            }
            catch { }

            return false;
        }

        public void Reset()
        {
            _shutdownInitiated = false;
        }
    }
}
