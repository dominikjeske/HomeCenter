using System.Collections.Generic;
using System.Reactive;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Events;
using Wirehome.Model.Extensions;

namespace Wirehome.Motion.Model
{
    public class DisableAutomationDecoder : IEventDecoder
    {
        private Room _room;

        public void DecodeMessage(IList<Timestamped<Event>> powerStateEvents)
        {
            if (powerStateEvents.Count < 3) return;

            int searchState = 1;

            foreach(var ev in powerStateEvents)
            {
                if (searchState == 1 && ev.Value[EventProperties.NewValue].ToStringValue() == PowerStateValue.ON)
                {
                    searchState = 2;
                    continue;
                }
                else if (searchState == 2 && ev.Value[EventProperties.NewValue].ToStringValue() == PowerStateValue.OFF)
                {
                    searchState = 3;
                    continue;
                }
                else if (searchState == 3 && ev.Value[EventProperties.NewValue].ToStringValue() == PowerStateValue.ON)
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