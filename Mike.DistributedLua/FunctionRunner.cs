using System.Collections.Generic;
using LuaInterface;

namespace Mike.DistributedLua
{
    public class FunctionRunner
    {
        private readonly IDictionary<string, FunctionInfo> functions = new Dictionary<string, FunctionInfo>();
        private readonly Lua lua;
        private readonly LuaTableToClrTypeMapper mapper;

        public FunctionRunner(Lua lua)
        {
            this.lua = lua;
            mapper = new LuaTableToClrTypeMapper(lua);
        }

        public void StartFunction(string functionName, LuaTable inputLuaTable)
        {
            var functionInfo = functions[functionName];

            var inputInstance = mapper.LuaTableToClrType(functionInfo.InputType, inputLuaTable);

            functionInfo.Function(inputInstance, outputInstance =>
                {
                    var outputLuaTable = mapper.ClrTypeToLuaTable(functionInfo.OutputType, outputInstance);

                    lua["LUA_RUNTIME_OPERATION_RESULT"] = outputLuaTable;
                    lua.DoString("coroutine.resume(co)");
                });
        }

        public void AddFunction(FunctionInfo functionInfo)
        {
            functions.Add(functionInfo.FunctionName, functionInfo);
        }
    }
}