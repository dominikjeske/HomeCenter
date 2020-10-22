using System.Threading.Tasks;
using HomeCenter.Abstractions;

namespace HomeCenter.Conditions
{
    public abstract class Condition : BaseObject, IValidable
    {
        public abstract Task<bool> Validate();

        public bool IsInverted { get; set; }
    }
}