using System.Text.Json.Serialization;

namespace HomeCenter.Adapters.Sony.Messages
{
    public class ActRegister1Request
    {
        [JsonPropertyName("function")]
        public System.String Function { get; set; }

        [JsonPropertyName("value")]
        public System.String Value { get; set; }

        public ActRegister1Request()
        {
        }

        public ActRegister1Request(System.String @function, System.String @value)
        {
            this.Function = @function;
            this.Value = @value;
        }
    }
}