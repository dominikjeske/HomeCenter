namespace HomeCenter.Services.MotionService.Model
{
    public class VisitType
    {
        public double Value { get; private set; }

        private VisitType(double value)
        {
            Value = value;
        }

        public static readonly VisitType None = new VisitType(0);
        public static readonly VisitType PassThru = new VisitType(1);
        public static readonly VisitType ShortVisit = new VisitType(2);
        public static readonly VisitType LongerVisit = new VisitType(3);
    }
}