using System;
using System.Collections;
using LuaInterface;

namespace Mike.DistributedLua
{
    public class PlutoSpike
    {
        public void Serialize()
        {
            Run(
@"

require('pluto')
print('pluto version '..pluto.version())
pluto.human(false)

t = { a = 'Hello World', b = 10 }

buf = pluto.persist({}, t)

outfile = io.open(persistFilePath, 'wb')
outfile:write(buf)
outfile:close()
");
        }

        public void DeSerialize()
        {
            Run(
@"

require('pluto')
print('pluto version '..pluto.version())
pluto.human(false)

infile, err = io.open(persistFilePath, 'rb')
if infile == nil then
	error('While opening: ' .. (err or 'no error'))
end

buf, err = infile:read('*a')
if buf == nil then
	error('While reading: ' .. (err or 'no error'))
end

infile:close()


t2 = pluto.unpersist({}, buf)

print(t2.a)
print(t2.b)

");
        }

        public void Error()
        {
            Run("error('This is an error in the script')");
        }

        public void SerializeCoRoutine()
        {
            Run(
@"
-- import pluto, print out the version number 
-- and set non-human binary serialization scheme.
require('pluto')
print('pluto version '..pluto.version())
pluto.human(false)

-- perms are items to be substituted at serialization
perms = { [coroutine.yield] = 1 }

-- the functions that we want to execute as a coroutine
function foo()
    local someMessage = 'And hello from a long dead variable!'
    local i = 4
    bar(someMessage)
    print(i)
end

function bar(msg)
    print('entered bar')
    -- bar runs to here then yields
    coroutine.yield()
    print(msg)
end

-- create and start the coroutine
co = coroutine.create(foo)
coroutine.resume(co)

-- the coroutine has now stopped at yield. so we can
-- persist its state with pluto
buf = pluto.persist(perms, co)

-- save the serialized state to a file
outfile = io.open(persistCRPath, 'wb')
outfile:write(buf)
outfile:close()

");
        }

        public void DeseralizeCoRoutine()
        {
            Run(
@"
-- import pluto, print out the version number 
-- and set non-human binary serialization scheme.
require('pluto')
print('pluto version '..pluto.version())
pluto.human(false)

-- perms are items to be substituted at serialization
-- (reverse the key/value pair that you used to serialize)
perms = { [1] = coroutine.yield }

-- get the serialized coroutine from disk
infile, err = io.open(persistCRPath, 'rb')
if infile == nil then
	error('While opening: ' .. (err or 'no error'))
end

buf, err = infile:read('*a')
if buf == nil then
	error('While reading: ' .. (err or 'no error'))
end

infile:close()

-- deserialize it
co = pluto.unpersist(perms, buf)

-- and run it
coroutine.resume(co)

");
        }

        public void Run(string script)
        {
            using (var lua = new Lua())
            {
                lua.RegisterFunction("print", null, GetType().GetMethod("Print"));

                lua["package.cpath"] = @"D:\Source\Mike.DistributedLua\Lua\?.dll";
                lua["persistFilePath"] = @"D:\Temp\test.plh";
                lua["persistCRPath"] = @"D:\Temp\cr.plh";

                lua.DoString(script);
            }
        }

        public static void Print(string message)
        {
            Console.WriteLine("LUA> " + message);
        }

        private static void PrintTable(LuaTable luaTable)
        {
            PrintTableInternal(luaTable, 0);
        }

        public static void PrintTableInternal(LuaTable table, int indent)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            Console.Out.WriteLine("Printing Table...");
            foreach (DictionaryEntry entry in table)
            {
                Console.Out.WriteLine("{0}{1} -> {2}", new string('\t', indent), entry.Key, entry.Value);
                var childTable = entry.Value as LuaTable;
                if (childTable != null)
                {
                    PrintTableInternal(childTable, ++indent);
                }
            }
        }
    }
}