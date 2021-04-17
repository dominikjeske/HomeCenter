using System.IO;
using System.Linq;
using System.Xml.Linq;
using HomeCenter.Abstractions;
using HomeCenter.Messages.Queries.Services;

namespace HomeCenter.Adapters.Denon.Messages
{
    internal class DenonControlQuery : HttpGetQuery, IFormatableMessage<DenonControlQuery>, IMessageResult<string, object>
    {
        public string? Command { get; init; }

        public string? Api { get; init; }

        public string? ReturnNode { get; init; }

        public string? Zone { get; init; }

        public bool Verify(string? input, object expectedResult) => (string)expectedResult == input;

        public override object Parse(string rawHttpResult)
        {
            using (var reader = new StringReader(rawHttpResult))
            {
                if (rawHttpResult?.Length == 0 && ReturnNode?.Length == 0)
                {
                    return "";
                }

                var xml = XDocument.Load(reader);
                var returnNode = xml.Descendants(ReturnNode).FirstOrDefault();
                return returnNode.Value;
            }
        }

        public DenonControlQuery FormatMessage()
        {
            Address = $"http://{Address}/goform/{Api}.xml?{Zone}{(Zone?.Length == 0 ? "" : "+")}{Command}";
            return this;
        }
    }
}