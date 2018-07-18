using System.Collections.Generic;
using System.Reactive;
using HomeCenter.ComponentModel.Events;

namespace HomeCenter.Motion.Model
{
    public interface IEventDecoder
    {
        void DecodeMessage(IList<Timestamped<Event>> powerStateEvents);
        void Init(Room room);
    }
}