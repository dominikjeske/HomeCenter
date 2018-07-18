using System.Collections.Generic;
using System.Reactive;
using Wirehome.ComponentModel.Events;

namespace Wirehome.Motion.Model
{
    public interface IEventDecoder
    {
        void DecodeMessage(IList<Timestamped<Event>> powerStateEvents);
        void Init(Room room);
    }
}