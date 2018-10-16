using System;

namespace HomeCenter.Adapters.Sony
{
    public class BraviaApiException : Exception
    {
        public int ErrorId { get; }

        public BraviaApiException(int errorId, string message)
            : base(message)
        {
            this.ErrorId = errorId;
        }
    }
}