using System;

namespace Gobchat
{
    public class ACTLogLine
    {
        public DateTime TimeStamp { get; }
        public string Message { get; }
        public ACTLogLine(DateTime timeStamp, string message)
        {
            this.TimeStamp = timeStamp;
            this.Message = message;
        }
    }
}
