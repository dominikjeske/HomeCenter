namespace HomeCenter.Services.MotionService.Tests
{
    public class MotionEnvelope : MessageEnvelope<MotionEvent>
    {
        public MotionEnvelope(string motionUid) : base(MotionEvent.Create(motionUid))
        {
        }
    }
}