using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace HomeCenter.Services.MotionService.Tests
{
    public class FakeLogger<T> : ILogger<T>, IDisposable
    {
        private const string MESSAGE_TEMPLATE = "{OriginalFormat}";
        private readonly IScheduler _scheduler;

        private DocumentStore _dbStore;
        private IDocumentSession _dbSession;

        public FakeLogger(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void InitLogger()
        {
            _dbStore = GetDbStore();

            _dbSession = _dbStore.OpenSession();

            ClearCurrentMessages();
        }

        private void ClearCurrentMessages()
        {
            var objects = _dbSession.Query<MoveInfo>();
            while (objects.Any())
            {
                foreach (var obj in objects)
                {
                    _dbSession.Delete(obj);
                }

                _dbSession.SaveChanges();
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Error)
            {
                throw exception;
            }
            var message = formatter(state, exception);

            Console.WriteLine($"[{_scheduler.Now:ss:fff}] {message}");

            var moveInfo = new MoveInfo
            {
                Time = _scheduler.Now,
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
                        moveInfo.Template = pair.Value?.ToString();
                    }
                    else
                    {
                        moveInfo.Properties.Add(pair.Key, pair.Value);
                    }
                }
            }

            _dbSession.Store(moveInfo);
        }

        private static DocumentStore GetDbStore()
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

            docStore.Conventions.CustomizeJsonSerializer = s => s.TypeNameHandling = TypeNameHandling.None;

            docStore.Initialize();
            return docStore;
        }

        public void Dispose()
        {
            _dbSession.SaveChanges();
            _dbSession.Dispose();
        }
    }
}