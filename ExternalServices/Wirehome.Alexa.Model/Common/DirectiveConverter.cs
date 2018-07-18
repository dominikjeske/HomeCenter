using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Wirehome.Alexa.Model.Discovery;

namespace Wirehome.Alexa.Model.Common
{
    public class DirectiveConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) => true;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            Directive directive = null;

            string headerName = jObject["header"]?["name"]?.Value<string>();
            string payloadVersion = jObject["header"]?["payloadVersion"]?.Value<string>();
            string headerNamespace = jObject["header"]?["namespace"]?.Value<string>();

            if (!String.IsNullOrEmpty(headerName) && !String.IsNullOrEmpty(payloadVersion) && !String.IsNullOrEmpty(headerNamespace))
            {
                if (payloadVersion == "3")
                {
                    directive = new Directive
                    {
                        Header = jObject["header"].ToObject<Header>()
                    };

                    directive.Endpoint = jObject["endpoint"]?.ToObject<Endpoint>();
                    directive.Payload = ReadPayload(headerName, jObject);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Type), $"Unsupported payload version: {payloadVersion}.");
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported smart home request: " + jObject);
            }
            
            return directive;
        }

        private Payload ReadPayload(string headerName, JObject jObject)
        {
            if(headerName == "Discover")
            {
                return jObject["payload"]?.ToObject<PayloadWithScope>();
            }

            return null;
        }
    }
}