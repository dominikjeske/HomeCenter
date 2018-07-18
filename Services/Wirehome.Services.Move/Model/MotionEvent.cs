namespace Wirehome.Motion.Model
{
    public class MotionEvent
    {
        public MotionEvent(string uid)
        {
            MotionDetectorUID = uid;
        }

        public string MotionDetectorUID { get; }
    }
}
