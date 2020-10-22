using System.Text.Json;
using HomeCenter.Abstractions;
using HomeCenter.Messages.Commands.Service;

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
            Body = JsonSerializer.Serialize(Message);

            return this;
        }
    }
}