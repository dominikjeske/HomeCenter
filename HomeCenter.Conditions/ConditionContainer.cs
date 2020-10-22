using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HomeCenter.Abstractions;

namespace HomeCenter.Conditions
{
    public class ConditionContainer : IValidable
    {
        private const string CONDITION_NAME = "C";
        private const string DEFAULT_OPERATOR = "AND";

        public string Expression { get; init; }
        public bool IsInverted { get; init; }
        public string DefaultOperator { get; init; } = DEFAULT_OPERATOR;
        public IList<IValidable> Conditions { get; init; } = new List<IValidable>();

        public async Task<bool> Validate()
        {
            var result = EvaluateExpression(await CheckConditions(BuildExpressionIfEmpty()));
            return IsInverted ? !result : result;
        }

        public ConditionContainer WithCondition(IValidable condition)
        {
            Conditions.Add(condition);
            return this;
        }

        private async Task<string> CheckConditions(StringBuilder expression)
        {
            int counter = 1;

            foreach (var condition in Conditions)
            {
                var result = await condition.Validate();
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
                        builder.Append($" {DefaultOperator} ");
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
            if (string.IsNullOrWhiteSpace(expression)) return true;

            var table = new System.Data.DataTable().Columns.Add("", typeof(bool), expression).Table;
            var r = table.NewRow();
            table.Rows.Add(r);
            return (bool)r[0];
        }
    }
}