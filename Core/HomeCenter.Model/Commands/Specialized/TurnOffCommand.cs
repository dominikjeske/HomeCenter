using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.Model.Commands.Specialized
{
    public class TurnOffCommand : Command
    {
        public static TurnOffCommand Default = new TurnOffCommand();
    }
}