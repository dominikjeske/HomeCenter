using System.Text.Json;
using HomeCenter.Abstractions;
using HomeCenter.Messages.Queries.Services;

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
            Body = JsonSerializer.Serialize(Message);

            return this;
        }
    }
}