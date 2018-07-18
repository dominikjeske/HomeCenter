using System.Threading.Tasks;

namespace Wirehome.Conditions
{
    public interface IValidable
    {
        Task<bool> Validate();

        bool IsInverted { get; }
    }
}