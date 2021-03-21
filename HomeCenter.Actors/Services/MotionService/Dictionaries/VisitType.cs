using CSharpFunctionalExtensions;

namespace HomeCenter.Services.MotionService.Model
{
    public sealed class VisitType : EnumValueObject<VisitType, int>
    {
        public static readonly VisitType None = new(nameof(None), 0);
        public static readonly VisitType PassThru = new(nameof(PassThru), 1);
        public static readonly VisitType ShortVisit = new(nameof(ShortVisit), 2);
        public static readonly VisitType LongerVisit = new(nameof(LongerVisit), 3);

        public VisitType(string name, int key) : base(key, name)
        {
        }

        public override string ToString() => $"{Name}[{Id}]";
    }
}