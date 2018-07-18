using System.Threading.Tasks;
using Wirehome.ComponentModel;

namespace Wirehome.Conditions
{
    public abstract class Condition : BaseObject, IValidable
    {
        public abstract Task<bool> Validate();

        public bool IsInverted { get; set; }
    }
}