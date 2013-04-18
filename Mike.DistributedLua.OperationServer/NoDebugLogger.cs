using System;
using EasyNetQ;

namespace Mike.DistributedLua.OperationServer
{
    public class NoDebugLogger : IEasyNetQLogger
    {
        public void DebugWrite(string format, params object[] args)
        {
            // do nothing
        }

        public void InfoWrite(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void ErrorWrite(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void ErrorWrite(Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}