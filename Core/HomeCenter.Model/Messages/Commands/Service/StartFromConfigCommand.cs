namespace HomeCenter.Model.Messages.Commands.Service
{
    public class StartSystemCommand : Command
    {
        public string AdapterMode
        {
            get => AsString(nameof(AdapterMode), "Embedded");
            set => SetProperty(nameof(AdapterMode), value);
        }

        public static StartSystemCommand Create(string mode = "Embedded") => new StartSystemCommand { AdapterMode = mode };
    }

   
}