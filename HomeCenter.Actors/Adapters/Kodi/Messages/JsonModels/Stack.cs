namespace HomeCenter.Adapters.Kodi.Messages.JsonModels
{
    public class Stack
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public object Type { get; set; }
    }
}