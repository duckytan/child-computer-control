using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class NamedPipeServer
    {
        private readonly string _pipeName;
        private Thread _serverThread;
        private bool _isRunning;
        private NamedPipeServerStream? _server;

        public event Action<PipeMessage>? OnMessageReceived;

        public NamedPipeServer(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Start()
        {
            _isRunning = true;
            _serverThread = new Thread(ServerLoop);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _server?.Dispose();
            _serverThread?.Join(1000);
        }

        private void ServerLoop()
        {
            while (_isRunning)
            {
                try
                {
                    _server = NamedPipeServerStreamAcl.Create(
                        _pipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.None,
                        1024,
                        1024,
                        null,
                        System.Security.AccessControl.PipeSecurityAuditRule,
                        System.Security.AccessControl.AllowEveryone);

                    _server.WaitForConnection();
                    HandleClient(_server);
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void HandleClient(NamedPipeServerStream server)
        {
            try
            {
                var formatter = new BinaryFormatter();
                var message = (PipeMessage)formatter.Deserialize(server);

                OnMessageReceived?.Invoke(message);

                if (message is StatusMessage statusMessage)
                {
                    formatter.Serialize(server, statusMessage);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                server.Close();
            }
        }

        public void SendMessage(PipeMessage message)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", _pipeName);
                client.Connect(1000);
                var formatter = new BinaryFormatter();
                formatter.Serialize(client, message);
            }
            catch (Exception)
            {
            }
        }
    }
}
