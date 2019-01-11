using System.Collections.Generic;
using System.Reactive;

namespace Wirehome.Motion.Model
{
    public class DisableAutomationDecoder : IEventDecoder
    {
        private Room _room;

        public void DecodeMessage(IList<Timestamped<PowerStateChangeEvent>> powerStateEvents)
        {
            if (powerStateEvents.Count < 3) return;

            int searchState = 1;

            foreach (var ev in powerStateEvents)
            {
                if (searchState == 1 && ev.Value.Value)
                {
                    searchState = 2;
                    continue;
                }
                else if (searchState == 2 && !ev.Value.Value)
                {
                    searchState = 3;
                    continue;
                }
                else if (searchState == 3 && ev.Value.Value)
                {
                    _room.DisableAutomation();
                    break;
                }
            }
        }

        public void Init(Room room)
        {
            _room = room;
        }
    }
}