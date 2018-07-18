using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using Wirehome.Core.Interface.Messaging;

namespace Wirehome.ComponentModel.Adapters.Sony
{
    public class SonyJsonMessage : HttpMessage
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public object Params { get; set; }
        public int Id { get; set; } = 1;
        public string Version { get; set; } = "1.0";

        public string AuthorisationKey { get; set; }

        public SonyJsonMessage()
        {
            RequestType = "POST";
        }

        public override string MessageAddress()
        {
            Cookies = new CookieContainer();
            Cookies.Add(new Uri($"http://{Address}/sony/"), new Cookie("auth", AuthorisationKey, "/sony", Address));
            return $"http://{Address}/sony/{Path}";
        }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(new
            {
                @method = Method,
                @params = Params == null ? new object[] { } : new object[] { Params },
                @id = Id,
                @version = Version,
            });
        }

        public override object ParseResult(string responseData, Type responseType = null)
        {
            var response = (JObject)JsonConvert.DeserializeObject(responseData);

            var error = response.GetValue("error");
            if (error != null)
            {
                throw new BraviaApiException((int)error[0], (string)error[1]);
            }

            var results = response.GetValue("results");
            if (results != null)
            {
                return results.ToObject(responseType);
            }
            else
            {
                return response.GetValue("result").First.ToString();
                //if (typeof(TResponse).GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICompositeResponse)))
                ////if (typeof(TResponse).GetTypeInfo().GetInterfaces().Contains(typeof(ICompositeResponse)))
                //{
                //    var obj = Activator.CreateInstance<TResponse>() as ICompositeResponse;
                //    obj.ReadFromJson((JArray)response.GetValue("result"));
                //    return (TResponse)obj;
                //}
                //else
                //{
                //try
                //{
                //    var test = response.GetValue("result").First.ToString();
                //}
                //catch (Exception ee)
                //{
                //}

                //return response.GetValue("result").First.ToObject(responseType);
                //}
            }
        }
    }

    public class SonyAudioVolumeRequest
    {
        [JsonProperty("target")]
        public System.String Target { get; set; }

        [JsonProperty("volume")]
        public System.String Volume { get; set; }

        public SonyAudioVolumeRequest()
        {
        }

        public SonyAudioVolumeRequest(System.String @target, System.String @volume)
        {
            this.Target = @target;
            this.Volume = @volume;
        }
    }

    public class SonyAudioMuteRequest
    {
        [JsonProperty("status")]
        public System.Boolean Status { get; set; }

        public SonyAudioMuteRequest()
        {
        }

        public SonyAudioMuteRequest(System.Boolean @status)
        {
            this.Status = @status;
        }
    }
}