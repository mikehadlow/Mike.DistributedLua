using System;
using System.Collections.Generic;

namespace Mike.DistributedLua.Messages
{
    public class ManifestRequest
    {
        public string FlightNumber { get; set; }
        public DateTime Date { get; set; }
    }

    public class Manifest
    {
        public string FlightNumber { get; set; }
        public DateTime Date { get; set; }
        public List<string> PnrNumbers { get; set; }

        public Manifest()
        {
            PnrNumbers = new List<string>();
        }
    }

    public class PnrRequest
    {
        public string PnrNumber { get; set; }
    }

    public class Pnr
    {
        public string PnrNumber { get; set; }
        public string EmailAddress { get; set; }
        public List<FlightLeg> FlightLegs { get; set; }

        public Pnr()
        {
            FlightLegs = new List<FlightLeg>();
        }
    }

    public class FlightLeg
    {
        public string FlightNumber { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }

    public class RenderRequest<TData>
    {
        public string TemplateId { get; set; }
        public TData Data { get; set; }
    }

    public class RenderResponse
    {
        public string Content { get; set; }
    }

    public class EmailRequest
    {
        public string ToAddress { get; set; }
        public string FromAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class EmailResponse
    {
        public bool Success { get; set; }
    }
}