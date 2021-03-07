using HomeCenter.Storage.RavenDB;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Text;

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
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RxTime", $"{_scheduler.Now:ss:fff}"));
        }

    }
}