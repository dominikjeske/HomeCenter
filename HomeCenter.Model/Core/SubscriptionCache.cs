using CSharpFunctionalExtensions;
using Proto;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Core
{
    public class SubscriptionCache : ValueObject
    {
        public SubscriptionCache(PID iD, Type type)
        {
            ID = iD;
            Type = type;
        }

        public PID ID { get; set; }
        public Type Type { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ID;
            yield return Type;
        }
    }
}