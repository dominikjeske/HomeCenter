namespace HomeCenter.Model.Messages.Commands
{
    public class Command : ActorMessage
    {
        public Command() { }

        public Command(string commandType, string uid)
        {
            Type = commandType;
            Uid = uid;
        }
    }
}