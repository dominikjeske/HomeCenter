using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.Model.Commands.Specialized
{
    public class RefreshLightCommand : Command
    {
        public static RefreshLightCommand Default = new RefreshLightCommand();
    }
}