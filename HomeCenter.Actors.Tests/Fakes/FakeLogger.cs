using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using HomeCenter.Services.MotionService.Tests;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace HomeCenter.Actors.Tests.Fakes
{
    public class FakeLogger<T> : ILogger<T>, IDisposable
    {
        private const string MESSAGE_TEMPLATE = "{OriginalFormat}";
        private readonly IScheduler _scheduler;

        private DocumentStore _dbStore;
        private IDocumentSession _dbSession;
        private readonly bool _useRavenDB;
        private object locki = new object();

        public FakeLogger(IScheduler scheduler, bool useRavenDB)
        {
            _scheduler = scheduler;
            _useRavenDB = useRavenDB;

            if (_useRavenDB)
            {
                InitRavenDB();
            }
        }

        private void InitRavenDB()
        {
            _dbStore = GetDbStore();

            _dbSession = _dbStore.OpenSession();

            ClearCurrentMessages();
        }

        private void ClearCurrentMessages()
        {
            //TODO DNF
            //var objects = _dbSession.Query<MoveInfo>();
            //while (objects.Any())
            //{
            //    foreach (var obj in objects)
            //    {
            //        _dbSession.Delete(obj);
            //    }

            //    _dbSession.SaveChanges();
            //}
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
            var time = $"{_scheduler.Now:ss:fff}";

            Console.WriteLine($"[{time}] {message}");

            if (_useRavenDB)
            {
                Task.Run(() =>
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
                                moveInfo.Template = pair.Value?.ToString();
                            }
                            else
                            {
                                moveInfo.Properties.Add(pair.Key, pair.Value);
                            }
                        }
                    }

                    lock (locki)
                    {
                        _dbSession.Store(moveInfo);
                    }
                });
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

            //TODO - fix
            //docStore.Conventions.CustomizeJsonSerializer = s => s.TypeNameHandling = TypeNameHandling.None;

            docStore.Initialize();
            return docStore;
        }

        public void Dispose()
        {
            Task.Run(() =>
            {
                lock (locki)
                {
                    _dbSession?.SaveChanges();
                    _dbSession?.Dispose();
                }
            });
        }
    }
}