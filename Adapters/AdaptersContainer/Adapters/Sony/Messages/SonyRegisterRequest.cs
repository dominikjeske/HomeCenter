using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Wirehome.Core.Interface.Messaging;

namespace Wirehome.ComponentModel.Adapters.Sony
{
    public partial class SonyRegisterRequest : HttpMessage
    {
        public string ClientID { get; set; } = "faed787d-8cac-4b67-8c0d-4e291584843b";
        public string ApplicationID { get; set; } = "Wirehome";
        public string PIN { get; set; }

        public SonyRegisterRequest()
        {
            RequestType = "POST";
        }

        public override string MessageAddress()
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                throw new Exception("Address cannot be null");
            }

            if (!string.IsNullOrWhiteSpace(PIN))
            {
                AuthorisationHeader = new KeyValuePair<string, string>("Basic", Convert.ToBase64String(new UTF8Encoding().GetBytes(":" + PIN)));
            }

            Cookies = new CookieContainer();
            Cookies.Add(new Uri($"http://{Address}/sony/"), new Cookie("auth", "", "/sony", Address));

            return $"http://{Address}/sony/accessControl";
        }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(new
            {
                @method = "actRegister",
                @params = new object[] { new ActRegisterRequest(ClientID, ApplicationID, "private"), new[] { new ActRegister1Request("WOL", "yes") } },
                @id = 1,
                @version = "1.0",
            });
        }

        public string ReadAuthKey()
        {
            return Cookies.GetCookies(new Uri($"http://{Address}/sony/"))
                          .OfType<Cookie>()
                          .First(x => x.Name == "auth")
                          .Value;
        }
    }
}