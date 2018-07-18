using Newtonsoft.Json;

namespace Wirehome.ComponentModel.Adapters.Sony
{
    public partial class SonyRegisterRequest
    {
        public class ActRegister1Request
        {
            [JsonProperty("function")]
            public System.String Function { get; set; }

            [JsonProperty("value")]
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
}