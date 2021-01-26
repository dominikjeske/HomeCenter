using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.ExceptionServices;

namespace HomeCenter.Actors.Tests.Fakes
{
    public class FakeLogger<T> : ILogger<T>, IDisposable
    {
        private const string MESSAGE_TEMPLATE = "{OriginalFormat}";
        private readonly IScheduler? _scheduler;
        private readonly bool _useRavenDB;
        private readonly object _locki = new object();

        private DocumentStore? _dbStore;
        private IDocumentSession? _dbSession;

        public FakeLogger() : this(null, false, false)
        {

        }


        public FakeLogger(IScheduler? scheduler, bool useRavenDB, bool clearLogs)
        {
            _scheduler = scheduler;
            _useRavenDB = useRavenDB;

            if (_useRavenDB)
            {
                InitRavenDB(clearLogs);
            }
        }

        private void InitRavenDB(bool clearLogs)
        {
            _dbStore = GetDbStore();

            _dbSession = _dbStore.OpenSession();

            if (clearLogs)
            {
                ClearCurrentMessages();
            }
        }

        private void ClearCurrentMessages()
        {
            var objects = _dbSession?.Query<MoveInfo>();
            while (objects?.Any() ?? false)
            {
                foreach (var obj in objects)
                {
                    _dbSession?.Delete(obj);
                }

                _dbSession?.SaveChanges();
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (_scheduler is null) return;

            var message = formatter(state, exception);
            var time = $"{_scheduler.Now:ss:fff}";

            Console.WriteLine($"[{time}] {message}");

            if (_useRavenDB)
            {
                var moveInfo = new MoveInfo
                {
                    Time = time,
                    LogLevel = logLevel,
                    EventId = eventId,
                    Exception = exception,
                    Message = message
                };

                if (state is IEnumerable<KeyValuePair<string, object>> list)
                {
                    foreach (var pair in list)
                    {
                        if (pair.Key == MESSAGE_TEMPLATE)
                        {
                            moveInfo.Template = pair.Value?.ToString() ?? "";
                        }
                        else
                        {
                            moveInfo.Properties.Add(pair.Key, pair.Value?.ToString() ?? "");
                        }
                    }
                }

                lock (_locki)
                {
                    _dbSession?.Store(moveInfo);
                }
            }

            if (logLevel == LogLevel.Error)
            {
                var exceptionDispatch = ExceptionDispatchInfo.Capture(exception);
                exceptionDispatch.Throw();
            }
        }

        private DocumentStore GetDbStore()
        {
            var docStore = new DocumentStore
            {
                Urls = new[]
                            {
                    "http://127.0.0.1:8080"
                },
                Database = "Moves",
                Conventions = { }
            };

            docStore.Initialize();
            return docStore;
        }

        public void Dispose()
        {
            _dbSession?.SaveChanges();
            _dbSession?.Dispose();
        }
    }
}