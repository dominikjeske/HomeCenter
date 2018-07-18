using HomeCenter.Conditions;
using HomeCenter.Contracts.Conditions;

namespace HomeCenter.Motion.Model
{
    public class IsTurnOffAutomaionCondition : Condition
    {
        private readonly Room _motionDescriptor;

        public IsTurnOffAutomaionCondition(Room motionDescriptor)
        {
            _motionDescriptor = motionDescriptor;

            WithExpression(() => _motionDescriptor.AreaDescriptor.TurnOffAutomationDisabled ? ConditionState.NotFulfilled : ConditionState.Fulfilled);
        }
    }
}