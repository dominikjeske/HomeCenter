namespace HomeCenter.Actors.Tests.Fakes
{
    public class FakeMotionLamp
    {
        public bool IsTurnedOn { get; private set; }

        public FakeMotionLamp(string id)
        {
            Id = id;
        }

        public void SetState(bool state)
        {
            IsTurnedOn = state;
        }

        public string Id { get; }

        // public void SetPowerStateSource(IObservable<PowerStateChangeEvent> source) => PowerStateChange = source;
        public override string ToString() => $"{Id} : {IsTurnedOn}";
    }
}