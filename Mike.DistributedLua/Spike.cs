using System;
using System.IO;
using System.Threading;

namespace Mike.DistributedLua
{
    public class Spike
    {
        public void RunDistributedLuaScript()
        {
            //var script = ReadResource("Mike.DistributedLua.LuaScript.lua");
            var script = ReadResource("Mike.DistributedLua.TicketingScript.lua");

            using (var ticketingProxy = new TicketingProxy())
            using (var runtime = new LuaRuntime(ticketingProxy.GetFunctions()))
            {
                runtime.Execute(script);

                Thread.Sleep(3000);
                Console.WriteLine("[UNIT TEST] Completed");
            }
        }

        private string ReadResource(string path)
        {
            using (var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(path)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}