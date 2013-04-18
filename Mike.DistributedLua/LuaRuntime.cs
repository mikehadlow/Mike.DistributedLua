using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace Mike.DistributedLua
{
    public class LuaRuntime : IDisposable
    {
        private readonly Lua lua = new Lua();
        private readonly FunctionRunner functionRunner;

        private const string functionTemplate =
            @"
function {0}(a) 
    return remoteFunction('{0}', a) 
end
";

        public LuaRuntime(IEnumerable<FunctionInfo> functions)
        {
            lua["package.cpath"] = @"D:\Source\Mike.DistributedLua\Lua\?.dll";

            functionRunner = new FunctionRunner(lua);
            foreach (var functionInfo in functions)
            {
                functionRunner.AddFunction(functionInfo);
                lua.DoString(string.Format(functionTemplate, functionInfo.FunctionName));
            }

            lua.RegisterFunction("print", this, GetType().GetMethod("Print"));
            lua.RegisterFunction("printTable", this, GetType().GetMethod("PrintTable"));
            lua.RegisterFunction("startFunction", functionRunner, functionRunner.GetType().GetMethod("StartFunction"));

            lua.DoString(
@"

function remoteFunction(name, input)
    startFunction(name, input)

    local cor = coroutine.running()
    coroutine.yield(cor)

    return LUA_RUNTIME_OPERATION_RESULT
end

");
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
        }

        public void Print(string msg)
        {
            Console.WriteLine("LUA> {0}", msg);
        }

        public void PrintTable(LuaTable table)
        {
            PrintTableInternal(table, 0);
        }

        private void PrintTableInternal(LuaTable table, int indent)
        {
            foreach (DictionaryEntry entry in table)
            {
                Print(string.Format("{0}{1} -> {2}", new string('\t', indent), entry.Key, entry.Value));

                var childTable = entry.Value as LuaTable;
                if (childTable != null)
                {
                    PrintTableInternal(childTable, ++indent);
                }
            }
        }
    }
}