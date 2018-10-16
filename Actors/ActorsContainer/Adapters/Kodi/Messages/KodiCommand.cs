using HomeCenter.Adapters.Kodi.Messages.JsonModels;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using Newtonsoft.Json;
using System;

namespace HomeCenter.Adapters.Kodi.Messages
{
    //https://github.com/FabienLavocat/kodi-remote/tree/master/src/KodiRemote.Core
    //https://github.com/akshay2000/XBMCRemoteRT/blob/master/XBMCRemoteRT/XBMCRemoteRT.Shared/RPCWrappers/Player.cs
    //http://kodi.wiki/view/JSON-RPC_API/Examples
    //http://kodi.wiki/view/JSON-RPC_API/v8#Notifications_2

    public class KodiCommand : HttpCommand, IFormatableMessage<KodiCommand>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Method { get; set; }
        public int Port { get; set; }
        public object Parameters { get; set; }

        public KodiCommand()
        {
            ContentType = "application/json-rpc";
        }

        public KodiCommand FormatMessage()
        {
            Creditionals = new System.Net.NetworkCredential(UserName, Password);
            Address = $"http://{Address}:{Port}/jsonrpc";

            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = Method,
                Parameters = Parameters
            };

            Body = JsonConvert.SerializeObject(jsonRpcRequest);

            return this;
        }

        public string ParseResult(string responseData, Type responseType = null)
        {
            var result = JsonConvert.DeserializeObject<JsonRpcResponse>(responseData);

            if (result.Error != null) throw new UnsupportedResultException(result.Error.ToString());

            return result.Result.ToString();
        }
    }
}