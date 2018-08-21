using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.Model.Commands.Specialized
{
    public class TurnOnCommand : Command
    {
        public static TurnOnCommand Default = new TurnOnCommand();
    }
}