using System.Threading.Tasks;
using HomeCenter.Model.Conditions;

namespace Wirehome.Motion.Model
{
    public class IsEnabledAutomationCondition : Condition
    {
        private readonly Room _motionDescriptor;

        public IsEnabledAutomationCondition(Room motionDescriptor)
        {
            _motionDescriptor = motionDescriptor;

            //TODO
            //WithExpression(() => _motionDescriptor.AutomationDisabled ? ConditionState.NotFulfilled : ConditionState.Fulfilled);
        }

        public override Task<bool> Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}