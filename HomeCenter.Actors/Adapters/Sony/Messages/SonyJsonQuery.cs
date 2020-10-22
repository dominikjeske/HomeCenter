using System;
using System.Net;
using System.Text.Json;
using HomeCenter.Abstractions;
using HomeCenter.Messages.Queries.Services;

namespace HomeCenter.Adapters.Sony.Messages
{
    public class SonyAudioResult
    {
    }

    public class SonyPowerResult
    {
    }

    public class SonyJsonQuery : HttpPostQuery, IFormatableMessage<SonyJsonQuery>
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public object Params { get; set; }
        public int Id { get; set; } = 1;
        public string Version { get; set; } = "1.0";
        public string AuthorisationKey { get; set; }

        public SonyJsonQuery FormatMessage()
        {
            Cookies = new CookieContainer();
            Cookies.Add(new Uri($"http://{Address}/sony/"), new Cookie("auth", AuthorisationKey, "/sony", Address));
            Address = $"http://{Address}/sony/{Path}";
            Body = JsonSerializer.Serialize(new
            {
                @method = Method,
                @params = Params == null ? new object[] { } : new object[] { Params },
                @id = Id,
                @version = Version,
            });

            return this;
        }

        //public object ParseResult(string responseData, Type responseType = null)
        //{
        //    var response = (JObject)JsonConvert.DeserializeObject(responseData);

        //    var error = response.GetValue("error");
        //    if (error != null)
        //    {
        //        throw new BraviaApiException((int)error[0], (string)error[1]);
        //    }

        //    var results = response.GetValue("results");
        //    if (results != null)
        //    {
        //        return results.ToObject(responseType);
        //    }
        //    else
        //    {
        //        return response.GetValue("result").First.ToString();
        //        //if (typeof(TResponse).GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICompositeResponse)))
        //        ////if (typeof(TResponse).GetTypeInfo().GetInterfaces().Contains(typeof(ICompositeResponse)))
        //        //{
        //        //    var obj = Activator.CreateInstance<TResponse>() as ICompositeResponse;
        //        //    obj.ReadFromJson((JArray)response.GetValue("result"));
        //        //    return (TResponse)obj;
        //        //}
        //        //else
        //        //{
        //        //try
        //        //{
        //        //    var test = response.GetValue("result").First.ToString();
        //        //}
        //        //catch (Exception ee)
        //        //{
        //        //}

        //        //return response.GetValue("result").First.ToObject(responseType);
        //        //}
        //    }
        //}
    }
}