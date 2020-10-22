using HomeCenter.Extensions;
using HomeCenter.Model.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace HomeCenter.Services.MotionService
{
    public class EventsDecoder : IDisposable
    {
        private readonly IList<EventDescriptor> EventDescriptors = new List<EventDescriptor>();
        private readonly Subject<Event> _eventStream = new Subject<Event>();
        private readonly DisposeContainer _disposables = new DisposeContainer();
        private readonly TimeSpan _maxMessageTime;

        public void Dispose() => _disposables.Dispose();

        private void EventHandler(IMessageEnvelope<Event> ev) => _eventStream.OnNext(ev.Message);

        public void Start(IMessageBroker messageBroker, IConcurrencyProvider concurrencyProvider)
        {
            foreach (var eventDescription in EventDescriptors)
            {
                var attributes = eventDescription.Event.GetProperties().ToDictionary();
                _disposables.Add(messageBroker.SubscribeForEvent<Event>(EventHandler, new RoutingFilter(attributes)));
            }

            _disposables.Add(_eventStream.Timestamp()
                                         .Buffer(_eventStream, _ => Observable.Timer(_maxMessageTime, concurrencyProvider.Scheduler))
                                         .Subscribe(DecodeMessage)
                            );
        }

        private void DecodeMessage(IList<Timestamped<Event>> events)
        {
            if (events.Count < EventDescriptors.Count) return;
            // We anayle events only if first match to our pattern
            if (!events[0].Equals(EventDescriptors[0])) return;

            int foundIndex = 0;

            for (int i = 1; i < events.Count; i++)
            {
                if (!events[i].Equals(EventDescriptors[foundIndex]))
                {
                    foundIndex++;
                    continue;
                }
                else
                {
                    continue;
                }
            }

            if (foundIndex == EventDescriptors.Count - 1)
            {
                // action
            }
        }
    }
}