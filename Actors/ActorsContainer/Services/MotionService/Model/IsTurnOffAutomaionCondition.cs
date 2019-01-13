using System.Threading.Tasks;
using HomeCenter.Model.Conditions;

namespace HomeCenter.Services.MotionService.Model
{
    public class IsTurnOffAutomaionCondition : Condition
    {
        private readonly Room _motionDescriptor;

        public IsTurnOffAutomaionCondition(Room motionDescriptor)
        {
            _motionDescriptor = motionDescriptor;

            //TODO
            //WithExpression(() => _motionDescriptor.AreaDescriptor.TurnOffAutomationDisabled ? ConditionState.NotFulfilled : ConditionState.Fulfilled);
        }

        public override Task<bool> Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}