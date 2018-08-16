using System;

namespace HomeCenter.Model.Exceptions
{
    public class CompilationException : Exception
    {
        public CompilationException() : base()
        {
        }

        protected CompilationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public CompilationException(string message) : base(message)
        {
        }

        public CompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}