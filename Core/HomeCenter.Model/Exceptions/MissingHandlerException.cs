using System;

namespace HomeCenter.Model.Exceptions
{
    public class MissingHandlerException : Exception
    {
        public MissingHandlerException() : base()
        {
        }

        protected MissingHandlerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public MissingHandlerException(string message) : base(message)
        {
        }

        public MissingHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}