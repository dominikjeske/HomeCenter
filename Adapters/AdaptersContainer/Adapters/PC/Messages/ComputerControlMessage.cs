using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using Newtonsoft.Json;

namespace HomeCenter.Adapters.PC.Messages
{
    public class ComputerControlCommand : HttpCommand, IFormatableMessage<ComputerControlCommand>
    {
        public int Port { get; set; } = 5000;
        public string Service { get; set; }
        public object Message { get; set; }

        public ComputerControlCommand()
        {
            ContentType = "application/json";
        }

        public T ParseResult<T>(string responseData) => JsonConvert.DeserializeObject<T>(responseData);

        public ComputerControlCommand FormatMessage()
        {
            Address = $"http://{Address}:{Port}/api/{Service}";
            Body = JsonConvert.SerializeObject(Message);

            return this;
        }
    }
}