using System;

namespace Gobchat
{
    public class ACTParseException : Exception
    {
        public ACTParseException() : base(){}

        public ACTParseException(string message) : base(message) {}

        public ACTParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
