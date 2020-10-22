namespace HomeCenter.Adapters.Sony.Messages
{
    public class ActRegisterRequest
    {
        [JsonPropertyName("clientid")]
        public System.String Clientid { get; set; }

        [JsonPropertyName("nickname")]
        public System.String Nickname { get; set; }

        [JsonPropertyName("level")]
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