using System;
using System.Threading;

namespace ChildPCGuard.Agent
{
    internal class Program
    {
        private static Agent? _agent;
        private static ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            string agentRole = "AgentA";
            string partnerName = "AgentB";

            if (args.Length > 0)
            {
                if (args[0] == "--agent-a")
                {
                    agentRole = "AgentA";
                    partnerName = "AgentB";
                }
                else if (args[0] == "--agent-b")
                {
                    agentRole = "AgentB";
                    partnerName = "AgentA";
                }
            }

            _agent = new Agent(agentRole, partnerName);

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _shutdownEvent.Set();
            };

            _agent.Start();
            _shutdownEvent.WaitOne();
        }
    }
}
