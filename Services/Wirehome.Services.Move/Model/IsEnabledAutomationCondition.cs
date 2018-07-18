using Wirehome.Conditions;
using Wirehome.Contracts.Conditions;

namespace Wirehome.Motion.Model
{
    public class IsEnabledAutomationCondition : Condition
    {
        private readonly Room _motionDescriptor;

        public IsEnabledAutomationCondition(Room motionDescriptor)
        {
            _motionDescriptor = motionDescriptor;

            WithExpression(() => _motionDescriptor.AutomationDisabled ? ConditionState.NotFulfilled : ConditionState.Fulfilled);
        }
    }
}