using System;
using System.ServiceProcess;
using System.Threading;

namespace ChildPCGuard.GuardService
{
    internal static class Program
    {
        private static ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--console")
            {
                RunAsConsole();
            }
            else
            {
                ServiceBase[] ServicesToRun = new ServiceBase[]
                {
                    new GuardService()
                };
                ServiceBase.Run(ServicesToRun);
            }
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
    }
}
