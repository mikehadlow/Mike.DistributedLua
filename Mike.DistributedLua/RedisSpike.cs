using System;
using ServiceStack.Redis;

namespace Mike.DistributedLua
{
    public class RedisSpike
    {
        public void BasicStore()
        {
            var client = (IRedisClient)new RedisClient("ubuntu");

            client.Add("mikey", "Hi from C# :)");
            client.Save();

            Console.Out.WriteLine("Stored value");
        }

        public void BasicRetrieve()
        {
            var client = (IRedisClient)new RedisClient("ubuntu");

            var value = client.GetValue("mikey");

            Console.Out.WriteLine("value = {0}", value);
        }
    }
}