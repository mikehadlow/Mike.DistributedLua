namespace Mike.DistributedLua.Messages
{
    public class OperationRequest
    {
        public int A { get; set; }
        public int B { get; set; }
        public string Operation { get; set; }
    }

    public class OperationResponse
    {
        public int Result { get; set; }
        public bool ErrorOccured { get; set; }
        public string Error { get; set; }
    }
}