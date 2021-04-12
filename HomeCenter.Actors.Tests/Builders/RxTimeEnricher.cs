using Serilog.Core;
using Serilog.Events;
using System;
using System.Reactive.Concurrency;

namespace HomeCenter.Actors.Tests.Builders
{
    public class RxTimeEnricher : ILogEventEnricher
    {
        private readonly IScheduler _scheduler;

        public RxTimeEnricher(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            //logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RealTime", $"{DateTime.Now:ss:ffff}"));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RxTime", $"{_scheduler.Now:ss:ffff}"));
        }
    }
}