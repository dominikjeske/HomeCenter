using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ConditionContainerDTO
    {
        public ConditionContainerDTO(string expression, IList<ConditionDTO> conditions, bool isInverted, string defaultOperator)
        {
            Expression = expression;
            Conditions = conditions;
            IsInverted = isInverted;
            DefaultOperator = defaultOperator;
        }

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