using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using Newtonsoft.Json;

namespace HomeCenter.Adapters.PC.Messages
{
    public class ComputerCommand : HttpCommand, IFormatableMessage<ComputerCommand>
    {
        public int Port { get; set; } = 5000;
        public string Service { get; set; }
        public object Message { get; set; }

        public ComputerCommand()
        {
            ContentType = "application/json";
        }

        public ComputerCommand FormatMessage()
        {
            Address = $"http://{Address}:{Port}/api/{Service}";
            Body = JsonConvert.SerializeObject(Message);

            return this;
        }
    }
}