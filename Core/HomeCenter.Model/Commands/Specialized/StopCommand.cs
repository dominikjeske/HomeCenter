using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.Model.Commands.Specialized
{
    public class StopCommand : Command
    {
        public static StopCommand Default = new StopCommand();
    }
}