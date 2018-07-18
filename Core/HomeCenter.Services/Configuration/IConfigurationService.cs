using System.Threading.Tasks;
using HomeCenter.Core.Services.DependencyInjection;

namespace HomeCenter.ComponentModel.Configuration
{
    public interface IConfigurationService
    {
        HomeCenterConfiguration ReadConfiguration(AdapterMode adapterMode);
    }
}