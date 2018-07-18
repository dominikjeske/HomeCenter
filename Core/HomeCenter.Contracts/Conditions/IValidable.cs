using System.Threading.Tasks;

namespace HomeCenter.Conditions
{
    public interface IValidable
    {
        Task<bool> Validate();

        bool IsInverted { get; }
    }
}