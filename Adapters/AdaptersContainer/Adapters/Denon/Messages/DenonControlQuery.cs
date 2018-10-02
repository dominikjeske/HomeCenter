using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Queries.Services;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HomeCenter.Adapters.Denon.Messages
{
    internal class DenonControlQuery : HttpQuery, IFormatableMessage<DenonControlQuery>, IMessageResult<string, string>
    {
        public string Command { get; set; }
        public string Api { get; set; }
        public string ReturnNode { get; set; }
        public string Zone { get; set; }

        public bool Verify(string input, string expectedResult)
        {
            var parsed = Parse(input);
            return expectedResult == parsed;
        }

        public string Parse(string input)
        {
            using (var reader = new StringReader(input))
            {
                if (input?.Length == 0 && ReturnNode?.Length == 0) return "";

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