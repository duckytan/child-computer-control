using System;
using System.Net.Sockets;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class NtpValidator
    {
        private readonly AppConfiguration _config;
        private DateTime? _lastValidNtpTime;

        public NtpValidator(AppConfiguration config)
        {
            _config = config;
        }

        public bool ValidateTime()
        {
            foreach (var ntpServer in _config.NtpServers)
            {
                try
                {
                    var ntpTime = GetNtpTime(ntpServer);
                    if (ntpTime.HasValue)
                    {
                        _lastValidNtpTime = ntpTime;
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        private DateTime? GetNtpTime(string server)
        {
            try
            {
                using var client = new UdpClient(server, 123);
                var ntpData = new byte[48];
                ntpData[0] = 0x1B;

                client.Send(ntpData, ntpData.Length);
                client.Receive(ref ntpData);

                ulong timestamp = 0;
                timestamp = ((ulong)ntpData[40] << 24) | ((ulong)ntpData[41] << 16) |
                            ((ulong)ntpData[42] << 8) | ((ulong)ntpData[43]);
                timestamp ^= 0xFFFFFFFF00000000;

                var dateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds((long)timestamp).ToLocalTime();

                return dateTime;
            }
            catch
            {
                return null;
            }
        }

        public DateTime? GetCurrentNtpTime()
        {
            return _lastValidNtpTime;
        }

        public bool IsTimeTampered()
        {
            if (!_lastValidNtpTime.HasValue) return false;

            var localTime = DateTime.Now;
            var difference = Math.Abs((localTime - _lastValidNtpTime.Value).TotalMinutes);

            return difference > _config.NtpToleranceMinutes;
        }
    }
}
