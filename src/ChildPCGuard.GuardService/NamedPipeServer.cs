using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
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
            _serverThread?.Join(1000);
        }

        private void ServerLoop()
        {
            var pipeSecurity = CreatePipeSecurity();

            while (_isRunning)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        _pipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.None,
                        1024,
                        1024);
                    server.SetAccessControl(pipeSecurity);

                    server.WaitForConnection();
                    HandleClient(server);
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static PipeSecurity CreatePipeSecurity()
        {
            var security = new PipeSecurity();

            var systemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            var adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

            security.AddAccessRule(new PipeAccessRule(
                systemSid,
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                AccessControlType.Allow));

            security.AddAccessRule(new PipeAccessRule(
                adminSid,
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                AccessControlType.Allow));

            security.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Deny));

            return security;
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
