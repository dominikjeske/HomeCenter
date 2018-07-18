using HomeCenter.Conditions;
using HomeCenter.Contracts.Conditions;

namespace HomeCenter.Motion.Model
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