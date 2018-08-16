using System;

namespace HomeCenter.Messaging.Exceptions
{
    public class QueryException : Exception
    {
        public QueryException() : base()
        {
        }

        protected QueryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public QueryException(string message) : base(message)
        {
        }

        public QueryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}