namespace Signals.Unity.Validation
{
    internal class ResultEntry
    {
        public Status Status;
        public string Message;

        public ResultEntry(Status status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
