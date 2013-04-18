using System;
using System.Collections.Generic;
using EasyNetQ;
using Mike.DistributedLua.Messages;

namespace Mike.DistributedLua
{
    public class TicketingProxy : IDisposable
    {
        private readonly IBus bus;

        public TicketingProxy()
        {
            bus = RabbitHutch.CreateBus("host=localhost", 
                register => register.Register<IEasyNetQLogger>(sp => new NoDebugLogger()));
        }

        public IEnumerable<FunctionInfo> GetFunctions()
        {
            yield return CreateFunctionInfo<ManifestRequest, Manifest>("retrieveManifest");
            yield return CreateFunctionInfo<PnrRequest, Pnr>("retrievePnr");
            yield return CreateFunctionInfo<RenderRequest<Pnr>, RenderResponse>("render");
            yield return CreateFunctionInfo<EmailRequest, EmailResponse>("emailSend");
        }

        public FunctionInfo CreateFunctionInfo<TInput, TOutput>(string functionName)
        {
            return new FunctionInfo
                {
                    FunctionName = functionName,
                    InputType = typeof(TInput),
                    OutputType = typeof(TOutput),
                    Function = (input, resultCallback) =>
                        {
                            Action<TOutput> objectCallback = output =>
                                {
                                    resultCallback(output);
                                };
                            using (var channel = bus.OpenPublishChannel())
                            {
                                channel.Request((TInput)input, objectCallback);
                            }
                        }
                };
        }

        public void Dispose()
        {
            bus.Dispose();
        }
    }
}