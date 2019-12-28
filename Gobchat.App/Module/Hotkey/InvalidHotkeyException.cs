using System;

namespace Gobchat.Core.Module.Hotkey
{
    [Serializable]
    public sealed class InvalidHotkeyException : Exception
    {
        public int ErrorCode { get; }

        public InvalidHotkeyException(string message) : base(message)
        {
        }

        public InvalidHotkeyException(int errorCode, string hotkey, Exception innerException)
            : base($"Error [{errorCode}] - {hotkey}", innerException)
        {
            ErrorCode = errorCode;
        }

        public InvalidHotkeyException()
        {
        }

        private InvalidHotkeyException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}