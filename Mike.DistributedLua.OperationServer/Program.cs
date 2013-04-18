using System;
using EasyNetQ;
using Mike.DistributedLua.Messages;

namespace Mike.DistributedLua.OperationServer
{
    class Program
    {
        static void Main()
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost", 
                register => register.Register<IEasyNetQLogger>(sp => new NoDebugLogger())))
            {
                bus.Respond<ManifestRequest, Manifest>(RetrieveManifest);
                bus.Respond<PnrRequest,Pnr>(RetrievePnr);
                bus.Respond<RenderRequest<Pnr>,RenderResponse>(Render);
                bus.Respond<EmailRequest,EmailResponse>(EmailSend);

                Console.WriteLine("Waiting for requests. Hit return to end.");
                Console.ReadLine();
            }
        }

        public static void LogWrite(string format, params object[] args)
        {
            Console.WriteLine("[OP Server] " + format, args);
        }

        public static Manifest RetrieveManifest(ManifestRequest manifestRequest)
        {
            LogWrite("Getting Manifest for flight: {0}", manifestRequest.FlightNumber);

            return new Manifest
                {
                    Date = manifestRequest.Date,
                    FlightNumber = manifestRequest.FlightNumber,
                    PnrNumbers =
                        {
                            "abc1",
                            "abc2",
                            "abc3",
                        }
                };
        }

        public static Pnr RetrievePnr(PnrRequest pnrRequest)
        {
            LogWrite("Getting PNR {0}", pnrRequest.PnrNumber);

            return new Pnr
                {
                    EmailAddress = pnrRequest.PnrNumber + "@someaddress.com",
                    PnrNumber = pnrRequest.PnrNumber,
                    FlightLegs =
                        {
                            new FlightLeg
                                {
                                    FlightNumber = "ABC 123",
                                    DepartureTime = new DateTime(2012, 4, 10, 9, 0, 0),
                                    ArrivalTime = new DateTime(2012, 4, 10, 16, 0, 0),
                                },
                            new FlightLeg
                                {
                                    FlightNumber = "ABC 234",
                                    DepartureTime = new DateTime(2012, 4, 10, 16, 0, 0),
                                    ArrivalTime = new DateTime(2012, 4, 10, 22, 0, 0),
                                }
                        }
                };
        }

        public static RenderResponse Render(RenderRequest<Pnr> renderRequest)
        {
            LogWrite("Rendering with template '{0}'", renderRequest.TemplateId);

            return new RenderResponse
                {
                    Content = string.Format("My rendered Pnr for '{0}'", renderRequest.Data.PnrNumber)
                };
        }

        public static EmailResponse EmailSend(EmailRequest emailRequest)
        {
            LogWrite("Sending email '{0}' to {1}", emailRequest.Subject, emailRequest.ToAddress);

            return new EmailResponse
                {
                    Success = true
                };
        }
    }
}
