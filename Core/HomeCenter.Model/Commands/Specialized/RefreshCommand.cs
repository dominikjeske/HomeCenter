using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.Model.Commands.Specialized
{
    public class RefreshCommand : Command
    {
        public static RefreshCommand Default = new RefreshCommand();
    }
}