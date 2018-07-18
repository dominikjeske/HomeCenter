using Newtonsoft.Json;
using System;
using Wirehome.Core.Interface.Messaging;

namespace Wirehome.ComponentModel.Adapters.Kodi
{
    //https://github.com/FabienLavocat/kodi-remote/tree/master/src/KodiRemote.Core
    //https://github.com/akshay2000/XBMCRemoteRT/blob/master/XBMCRemoteRT/XBMCRemoteRT.Shared/RPCWrappers/Player.cs
    //http://kodi.wiki/view/JSON-RPC_API/Examples
    //http://kodi.wiki/view/JSON-RPC_API/v8#Notifications_2

    public class KodiMessage : HttpMessage
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Method { get; set; }
        public int Port { get; set; }
        public object Parameters { get; set; }

        public KodiMessage()
        {
            RequestType = "POST";
            ContentType = "application/json-rpc";
        }

        public override string MessageAddress()
        {
            Creditionals = new System.Net.NetworkCredential(UserName, Password);

            return $"http://{Address}:{Port}/jsonrpc";
        }

        public override string Serialize()
        {
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = Method,
                Parameters = Parameters
            };

            return JsonConvert.SerializeObject(jsonRpcRequest);
        }

        public override object ParseResult(string responseData, Type responseType = null)
        {
            var result = JsonConvert.DeserializeObject<JsonRpcResponse>(responseData);

            if (result.Error != null) throw new Exception(result.Error.ToString());

            return result.Result.ToString();
        }
    }
}