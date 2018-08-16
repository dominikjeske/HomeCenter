using System;

namespace HomeCenter.WindowsService.Core.Exceptions
{
    public class ProcessNotFoundException : Exception
    {
        public ProcessNotFoundException() : base()
        {
        }

        protected ProcessNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public ProcessNotFoundException(string message) : base(message)
        {
        }

        public ProcessNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}