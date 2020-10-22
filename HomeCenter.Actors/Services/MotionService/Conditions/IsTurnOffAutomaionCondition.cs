using System.Threading.Tasks;
using HomeCenter.Conditions;

namespace HomeCenter.Services.MotionService.Model
{
    internal class IsTurnOffAutomaionCondition : Condition
    {
        private readonly Room _motionDescriptor;

        public IsTurnOffAutomaionCondition(Room motionDescriptor)
        {
            _motionDescriptor = motionDescriptor;
        }

        public override Task<bool> Validate()
        {
            return Task.FromResult(!_motionDescriptor.AreaDescriptor.TurnOffAutomationDisabled);
        }
    }
}