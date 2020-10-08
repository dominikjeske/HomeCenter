using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ConditionContainerDTO
    {
        [JsonPropertyName("Expression")]
        public string Expression { get; set; }

        [JsonPropertyName("Conditions")]
        public IList<ConditionDTO> Conditions { get; set; }

        [JsonPropertyName("IsInverted")]
        public bool IsInverted { get; set; }

        [JsonPropertyName("DefaultOperator")]
        public string DefaultOperator { get; set; }
    }
}