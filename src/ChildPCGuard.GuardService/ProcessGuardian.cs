using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class ProcessGuardian
    {
        private readonly AppConfiguration _config;
        private readonly Dictionary<string, DateTime> _lastHeartbeat = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, Process> _agentProcesses = new Dictionary<string, Process>();
        private System.Threading.Timer _heartbeatCheckTimer;
        private bool _isRunning;

        private readonly string[] _agentPaths = new string[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "svchost.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "RuntimeBroker.exe")
        };

        public event Action<string>? OnAgentDead;

        public ProcessGuardian(AppConfiguration config)
        {
            _config = config;
        }

        public void StartGuardians()
        {
            _isRunning = true;

            for (int i = 0; i < _agentPaths.Length; i++)
            {
                string agentPath = _agentPaths[i];
                string agentName = Path.GetFileNameWithoutExtension(agentPath);
                StartAgent(agentName, agentPath, i == 0 ? "--agent-a" : "--agent-b");
            }

            _heartbeatCheckTimer = new System.Threading.Timer(CheckHeartbeats, null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10));
        }

        public void Stop()
        {
            _isRunning = false;
            _heartbeatCheckTimer?.Dispose();

            foreach (var process in _agentProcesses.Values)
            {
                try { process.Kill(); } catch { }
            }
            _agentProcesses.Clear();
        }

        public void ReceiveHeartbeat(string agentName)
        {
            _lastHeartbeat[agentName] = DateTime.Now;
        }

        private void StartAgent(string agentName, string agentPath, string argument)
        {
            try
            {
                if (!File.Exists(agentPath))
                {
                    string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{agentName}.exe");
                    if (File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, agentPath, true);
                    }
                }

                var psi = new ProcessStartInfo
                {
                    FileName = agentPath,
                    Arguments = argument,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                if (process != null)
                {
                    _agentProcesses[agentName] = process;
                    _lastHeartbeat[agentName] = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("ChildPCGuard", $"Failed to start agent {agentName}: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public void RestartAgent(string agentName)
        {
            if (!_isRunning) return;

            if (_agentProcesses.ContainsKey(agentName))
            {
                try { _agentProcesses[agentName].Kill(); } catch { }
                _agentProcesses.Remove(agentName);
            }

            string agentPath = _agentPaths.FirstOrDefault(p =>
                Path.GetFileNameWithoutExtension(p) == agentName);

            if (agentPath != null)
            {
                string argument = agentName == "svchost" ? "--agent-a" : "--agent-b";
                StartAgent(agentName, agentPath, argument);
            }
        }

        private void CheckHeartbeats(object state)
        {
            if (!_isRunning) return;

            var timeout = TimeSpan.FromSeconds(30);

            foreach (var kvp in _lastHeartbeat.ToList())
            {
                if (DateTime.Now - kvp.Value > timeout)
                {
                    System.Diagnostics.EventLog.WriteEntry("ChildPCGuard", $"Agent {kvp.Key} heartbeat timeout, restarting...", System.Diagnostics.EventLogEntryType.Warning);
                    OnAgentDead?.Invoke(kvp.Key);
                    RestartAgent(kvp.Key);
                }
            }

            foreach (var kvp in _agentProcesses.ToList())
            {
                if (kvp.Value.HasExited)
                {
                    System.Diagnostics.EventLog.WriteEntry("ChildPCGuard", $"Agent {kvp.Key} process exited, restarting...", System.Diagnostics.EventLogEntryType.Warning);
                    RestartAgent(kvp.Key);
                }
            }
        }
    }
}
