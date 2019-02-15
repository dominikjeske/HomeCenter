namespace HomeCenter.Model.Messages.Commands.Device
{
    public class TurnOnCommand : Command
    {
        public static TurnOnCommand Default = new TurnOnCommand();

        public static TurnOnCommand Create(int stateTime) => (TurnOnCommand)new TurnOnCommand().SetProperty(MessageProperties.StateTime, stateTime);
    }

}