using HomeCenter.Model.Core;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Services.Actors
{
    public interface ITypeLoader
    {
        IActor GetProxyType<T>(T actorConfig) where T : IBaseObject;

        Task LoadTypes();
    }
}