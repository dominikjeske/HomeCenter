namespace HomeCenter.Services.MotionService.Tests
{
    public class FakeMotionLamp
    {
        private bool isTurnedOn;

        public FakeMotionLamp(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public bool IsTurnedOn => isTurnedOn;

        private void SetIsTurnedOn(bool value)
        {
            if (value != isTurnedOn)
            {
                var powerStateValue = value ? true : false;

                //TODO
                //_powerStateSubject.OnNext(new PowerStateChangeEvent(powerStateValue, PowerStateChangeEvent.AutoSource));
            }
            isTurnedOn = value;
        }

        // public void SetPowerStateSource(IObservable<PowerStateChangeEvent> source) => PowerStateChange = source;
        public override string ToString() => $"{Id} : {IsTurnedOn}";
    }
}