using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace ChildPCGuard.GuardService
{
    internal static class Program
    {
        private static ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "--console")
                {
                    RunAsConsole();
                    return;
                }
                else if (args[0] == "--install")
                {
                    InstallService();
                    return;
                }
                else if (args[0] == "--uninstall")
                {
                    UninstallService();
                    return;
                }
            }

            ServiceBase[] ServicesToRun = new ServiceBase[]
            {
                new GuardService()
            };
            ServiceBase.Run(ServicesToRun);
        }

        static void RunAsConsole()
        {
            Console.WriteLine("ChildPCGuard Service starting in console mode...");
            var service = new GuardService();
            service.Start(new string[0]);

            Console.WriteLine("Service started. Press Ctrl+C to stop.");
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _shutdownEvent.Set();
            };

            _shutdownEvent.WaitOne();
            service.Stop();
            Console.WriteLine("Service stopped.");
        }

        static void InstallService()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = "create WinSecSvc_a1b2c3d4 binPath= \"\" start= auto DisplayName= \"Windows Security Update Service\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };
                Process.Start(psi).WaitForExit();
                Console.WriteLine("Service installed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to install service: {ex.Message}");
            }
        }

        static void UninstallService()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = "delete WinSecSvc_a1b2c3d4",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };
                Process.Start(psi).WaitForExit();
                Console.WriteLine("Service uninstalled successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to uninstall service: {ex.Message}");
            }
        }
    }
}
