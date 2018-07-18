using Wirehome.Conditions;
using Wirehome.Contracts.Conditions;

namespace Wirehome.Motion.Model
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