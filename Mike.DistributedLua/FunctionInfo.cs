using System;

namespace Mike.DistributedLua
{
    public class FunctionInfo
    {
        public string FunctionName { get; set; }
        public Type InputType { get; set; }
        public Type OutputType { get; set; }
        public Action<object, Action<object>> Function { get; set; }
    }
}