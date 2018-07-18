using System;

namespace HomeCenter.Core.EventAggregator
{
    public class WrongResultException : Exception
    {
        public WrongResultException(object actual, object excepted) : base($"Result value is {actual} but excepted value is {excepted}") { }
    }
}
