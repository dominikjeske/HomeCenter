using HomeCenter.Abstractions;
using HomeCenter.Messages.Events.Device;

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

        public override string ToString() => $"{Id} : {IsTurnedOn}";
    }
}