using Microsoft.Extensions.Logging;

namespace HomeCenter.Services.MotionService
{
    // TODO add unit test for unique number
    public static class MoveEventId
    {
        public const int MessageBase = 100;

        public static EventId Motion = new EventId(MessageBase, nameof(Motion));
        public static EventId VectorCancel = new EventId(MessageBase + 1, nameof(VectorCancel));
        public static EventId AutomationDisabled = new EventId(MessageBase + 2, nameof(AutomationDisabled));
        public static EventId AutomationEnabled = new EventId(MessageBase + 3, nameof(AutomationEnabled));
        public static EventId MarkVector = new EventId(MessageBase + 4, nameof(MarkVector));
        public static EventId ConfusedVector = new EventId(MessageBase + 5, nameof(ConfusedVector));
        public static EventId Probability = new EventId(MessageBase + 6, nameof(Probability));
        public static EventId Tuning = new EventId(MessageBase + 7, nameof(Tuning));
        public static EventId PowerState = new EventId(MessageBase + 8, nameof(PowerState));
        public static EventId MarkLeave= new EventId(MessageBase + 9, nameof(MarkLeave));
    }
}