using System.Collections.Generic;
using System.Reactive;

namespace Wirehome.Motion.Model
{
    public interface IEventDecoder
    {
        void DecodeMessage(IList<Timestamped<PowerStateChangeEvent>> powerStateEvents);
        void Init(Room room);
    }
}