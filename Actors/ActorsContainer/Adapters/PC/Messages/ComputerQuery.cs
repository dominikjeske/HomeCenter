using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using Newtonsoft.Json;

namespace HomeCenter.Adapters.PC.Messages
{
    public class ComputerQuery : HttpPostQuery, IFormatableMessage<ComputerQuery>
    {
        public int Port { get; set; } = 5000;
        public string Service { get; set; }
        public object Message { get; set; }

        public ComputerQuery()
        {
            ContentType = "application/json";
        }

        public ComputerQuery FormatMessage()
        {
            Address = $"http://{Address}:{Port}/api/{Service}";
            Body = JsonConvert.SerializeObject(Message);

            return this;
        }

        public override object Parse(string rawHttpResult) => rawHttpResult;
    }
}