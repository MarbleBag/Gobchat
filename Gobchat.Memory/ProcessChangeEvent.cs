namespace Gobchat.Memory
{
    public class ProcessChangeEvent : System.EventArgs
    {
        public bool IsProcessValid { get; }
        public int ProcessId { get; }

        public ProcessChangeEvent(bool isProcessValid, int processId)
        {
            IsProcessValid = isProcessValid;
            ProcessId = processId;
        }
    }
}