using System;
using System.Threading;
using EasyNetQ;
using Mike.DistributedLua.Messages;

namespace Mike.DistributedLua.OperationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                bus.Respond<OperationRequest, OperationResponse>(request =>
                    {
                        Console.WriteLine("Got calculating {0} {1} {2}",
                            request.A, request.Operation, request.B);

                        Thread.Sleep(1000);

                        var response = new OperationResponse();
                        switch (request.Operation)
                        {
                            case "+":
                                response.Result = request.A + request.B;
                                break;
                            case "-":
                                response.Result = request.A - request.B;
                                break;
                            case "*":
                                response.Result = request.A * request.B;
                                break;
                            case "/":
                                response.Result = request.A / request.B;
                                break;
                            default:
                                response.Error = "Unknown operation: " + request.Operation;
                                response.ErrorOccured = true;
                                break;
                        }
                        return response;
                    });

                Console.WriteLine("Waiting for requests. Hit return to end.");
                Console.ReadLine();
            }
        }
    }
}
