using System.Threading.Tasks;

namespace HomeCenter.Model.Conditions
{
    public interface IValidable
    {
        Task<bool> Validate();

        bool IsInverted { get; }
    }
}