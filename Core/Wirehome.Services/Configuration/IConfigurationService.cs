using System.Threading.Tasks;
using Wirehome.Core.Services.DependencyInjection;

namespace Wirehome.ComponentModel.Configuration
{
    public interface IConfigurationService
    {
        WirehomeConfiguration ReadConfiguration(AdapterMode adapterMode);
    }
}