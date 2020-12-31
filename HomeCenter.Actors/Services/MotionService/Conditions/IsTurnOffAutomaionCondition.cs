using System.Threading.Tasks;
using HomeCenter.Conditions;

namespace HomeCenter.Services.MotionService.Model
{
    internal class IsTurnOffAutomaionCondition : Condition
    {
        private readonly Room _room;

        public IsTurnOffAutomaionCondition(Room room)
        {
            _room = room;
        }

        public override Task<bool> Validate()
        {
            return Task.FromResult(!_room.AreaDescriptor.TurnOffAutomationDisabled);
        }
    }
}