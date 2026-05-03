using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ChildPCGuard.Shared;

namespace ChildPCGuard.Agent
{
    public class Agent
    {
        private readonly string _agentName;
        private readonly string _partnerName;
        private readonly string _pipeName = "ChildPCGuardPipe";
        private Process? _partnerProcess;
        private Timer? _heartbeatTimer;
        private Timer? _partnerCheckTimer;
        private bool _isRunning;

        public Agent(string agentName, string partnerName)
        {
            _agentName = agentName;
            _partnerName = partnerName;
        }

        public void Start()
        {
            _isRunning = true;

            StartPartnerProcess();

            _heartbeatTimer = new Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
            _partnerCheckTimer = new Timer(CheckPartnerProcess, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            Thread.Sleep(Timeout.Infinite);
        }

        private void StartPartnerProcess()
        {
            try
            {
                string system32Path = Environment.GetFolderPath(Environment.SpecialFolder.System);
                string partnerPath = Path.Combine(system32Path, $"{_partnerName}.exe");

                if (!File.Exists(partnerPath))
                {
                    return;
                }

                string argument = _partnerName == "svchost" ? "--agent-a" : "--agent-b";

                var psi = new ProcessStartInfo
                {
                    FileName = partnerPath,
                    Arguments = argument,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                _partnerProcess = Process.Start(psi);
            }
            catch { }
        }

        private void SendHeartbeat(object? state)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", _pipeName);
                client.Connect(1000);

                var heartbeat = new HeartbeatMessage();
                heartbeat.ProcessName = _agentName;
                heartbeat.ProcessId = (int)NativeAPI.GetCurrentProcessId();
                heartbeat.Timestamp = DateTime.Now;

                var formatter = new BinaryFormatter();
                formatter.Serialize(client, heartbeat);
            }
            catch { }
        }

        private void CheckPartnerProcess(object? state)
        {
            if (_partnerProcess == null || _partnerProcess.HasExited)
            {
                NotifyServicePartnerDead();
                StartPartnerProcess();
            }
        }

        private void NotifyServicePartnerDead()
        {
            try
            {
                using var client = new NamedPipeClientStream(".", _pipeName);
                client.Connect(1000);

                var message = new PipeMessage
                {
                    Type = PipeMessageType.RestartAgent,
                    ProcessName = _partnerName,
                    Timestamp = DateTime.Now
                };

                var formatter = new BinaryFormatter();
                formatter.Serialize(client, message);
            }
            catch { }
        }

        public void Stop()
        {
            _isRunning = false;
            _heartbeatTimer?.Dispose();
            _partnerCheckTimer?.Dispose();
            _partnerProcess?.Kill();
        }
    }
}
