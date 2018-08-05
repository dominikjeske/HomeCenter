using HomeCenter.Core.Services.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HomeCenter.Conditions
{
    public class ConditionContainer : IValidable
    {
        private const string CONDITION_NAME = "C";
        private const string DEFAULT_OPERATOR = "AND";

        [Map] public string Expression { get; set; } 
        [Map] public bool IsInverted { get; private set; }
        [Map] public string DefaultOperator { get; private set; } = DEFAULT_OPERATOR;
        [Map] public IList<IValidable> Conditions { get; private set; } = new List<IValidable>();

        public async Task<bool> Validate()
        {
            var result = EvaluateExpression(await CheckConditions(BuildExpressionIfEmpty()).ConfigureAwait(false));
            return IsInverted ? !result : result;
        }

        private async Task<string> CheckConditions(StringBuilder expression)
        {
            int counter = 1;

            foreach (var condition in Conditions)
            {
                var result = await condition.Validate().ConfigureAwait(false);
                expression.Replace($"{CONDITION_NAME}{counter++}", result.ToString());
            }

            return expression.ToString();
        }

        private StringBuilder BuildExpressionIfEmpty()
        {
            int counter = 1;
            StringBuilder builder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(Expression))
            {
                foreach (var condition in Conditions)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append($" {DEFAULT_OPERATOR} ");
                    }

                    builder.Append(CONDITION_NAME).Append(counter++);
                }
            }
            else
            {
                builder.Append(Expression);
            }

            counter = 1;
            foreach (var condition in Conditions)
            {
                if (condition.IsInverted)
                {
                    var conditionName = $"{CONDITION_NAME}{counter++}";
                    builder.Replace(conditionName, $"(not {conditionName})");
                }
            }

            return builder;
        }

        public bool EvaluateExpression(string expression)
        {
            var table = new System.Data.DataTable().Columns.Add("", typeof(bool), expression).Table;
            var r = table.NewRow();
            table.Rows.Add(r);
            return (bool)r[0];
        }
    }
}