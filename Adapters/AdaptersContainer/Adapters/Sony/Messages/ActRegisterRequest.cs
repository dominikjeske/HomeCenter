using Newtonsoft.Json;

namespace Wirehome.ComponentModel.Adapters.Sony
{
    public partial class SonyRegisterRequest
    {
        public class ActRegisterRequest
        {
            [JsonProperty("clientid")]
            public System.String Clientid { get; set; }

            [JsonProperty("nickname")]
            public System.String Nickname { get; set; }

            [JsonProperty("level")]
            public System.String Level { get; set; }

            public ActRegisterRequest()
            {
            }

            public ActRegisterRequest(System.String @clientid, System.String @nickname, System.String @level)
            {
                this.Clientid = @clientid;
                this.Nickname = @nickname;
                this.Level = @level;
            }
        }
    }
}