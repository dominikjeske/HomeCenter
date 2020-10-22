using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Model
{
    internal class IsEnabledAutomationCondition : Condition
    {
        private readonly Room _motionDescriptor;

        public IsEnabledAutomationCondition(Room motionDescriptor)
        {
            _motionDescriptor = motionDescriptor;
        }

        public override Task<bool> Validate()
        {
            return Task.FromResult(!_motionDescriptor.AutomationDisabled);
        }
    }
}