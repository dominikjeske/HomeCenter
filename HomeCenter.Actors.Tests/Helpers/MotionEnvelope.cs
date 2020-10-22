using HomeCenter.EventAggregator;
using HomeCenter.Messages.Events.Device;

namespace HomeCenter.Actors.Tests.Helpers
{
    public class MotionEnvelope : MessageEnvelope<MotionEvent>
    {
        public MotionEnvelope(string motionUid) : base(MotionEvent.Create(motionUid))
        {
        }
    }
}