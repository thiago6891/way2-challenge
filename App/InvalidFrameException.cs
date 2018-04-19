using System;
using System.Runtime.Serialization;

namespace App
{
    [Serializable]
    public class InvalidFrameException : Exception
    {
        public InvalidFrameException()
        {
        }

        public InvalidFrameException(string message) : base(message)
        {
        }

        public InvalidFrameException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidFrameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}