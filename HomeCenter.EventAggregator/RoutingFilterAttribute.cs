using System;

namespace HomeCenter.Broker
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RoutingFilterAttribute : Attribute
    {
        public RoutingFilterAttribute(string simpleFilter)
        {
            SimpleFilter = simpleFilter;
        }

        public string SimpleFilter { get; }

        public RoutingFilter ToMessageFilter() => new RoutingFilter(SimpleFilter);
    }
}