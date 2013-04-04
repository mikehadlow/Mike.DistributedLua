using System;
using System.IO;
using System.Threading;
using EasyNetQ;
using LuaInterface;
using Mike.DistributedLua.Messages;

namespace Mike.DistributedLua
{
    public class Spike
    {
        public void RunDistributedLuaScript()
        {
            var script = ReadResource("Mike.DistributedLua.LuaScript.lua");

            using (var runtime = new LuaRuntime())
            {
                runtime.Execute(script);

                Thread.Sleep(10000);
                Console.WriteLine("Completed");
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

    public class LuaRuntime : IDisposable
    {
        private readonly Lua lua = new Lua();
        private readonly Functions functions = new Functions();

        public LuaRuntime()
        {
            lua.RegisterFunction("print", functions, typeof(Functions).GetMethod("Print"));
            lua.RegisterFunction("startOperation", this, GetType().GetMethod("StartOperation"));

            lua.DoString(
@"
function remoteAdd(a, b) return remoteOperation(a, b, '+'); end
function remoteMultiply(a, b) return remoteOperation(a, b, '*'); end
function remoteDivide(a, b) return remoteOperation(a, b, '/'); end

function remoteOperation(a, b, op)
    startOperation(a, b, op)
    local cor = coroutine.running()
    coroutine.yield(cor)

    return LUA_RUNTIME_OPERATION_RESULT
end
");
        }

        public void StartOperation(int a, int b, string operation)
        {
            functions.RunOperation(a, b, operation, result =>
                {
                    lua["LUA_RUNTIME_OPERATION_RESULT"] = result;
                    lua.DoString("coroutine.resume(co)");
                });
        }

        public void Execute(string script)
        {
            const string coroutineWrapper =
@"co = coroutine.create(function() 
{0}
end)";
            lua.DoString(string.Format(coroutineWrapper, script));
            lua.DoString("coroutine.resume(co)");
        }

        public void Dispose()
        {
            lua.Dispose();
            functions.Dispose();
        }
    }

    public class Functions : IDisposable
    {
        private readonly IBus bus;

        public Functions()
        {
            bus = RabbitHutch.CreateBus("host=localhost");
        }

        public void Dispose()
        {
            bus.Dispose();
        }

        public void RunOperation(int a, int b, string operation, Action<int> resultCallback)
        {
            using (var channel = bus.OpenPublishChannel())
            {
                var request = new OperationRequest()
                    {
                        A = a,
                        B = b,
                        Operation = operation
                    };
                channel.Request<OperationRequest, OperationResponse>(request, response =>
                    {
                        Console.WriteLine("Got response {0}", response.Result);
                        resultCallback(response.Result);
                    });
            }
        }

        public void Print(string msg)
        {
            Console.WriteLine("LUA> {0}", msg);
        }
    }
}