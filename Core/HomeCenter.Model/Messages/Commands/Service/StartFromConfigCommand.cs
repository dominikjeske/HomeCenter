namespace HomeCenter.Model.Messages.Commands.Service
{
    public class StartSystemCommand : Command
    {
        public string AdapterMode
        {
            get => AsString(nameof(AdapterMode), "Embedded");
            set => SetProperty(nameof(AdapterMode), value);
        }


        public string Configuration
        {
            get => AsString(nameof(Configuration));
            set => SetProperty(nameof(Configuration), value);
        }

        public static StartSystemCommand Create(string configuration, string mode = "Embedded") => new StartSystemCommand { AdapterMode = mode, Configuration = configuration };
    }

   
}