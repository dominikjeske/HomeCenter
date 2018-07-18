using Microsoft.AspNetCore.Mvc;
using Wirehome.ComponentModel.Adapters.Pc;
using Wirehome.WindowsService.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        private readonly IAudioService _audioService;

        public StatusController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet]
        public ComputerStatus Get()
        {
            return new ComputerStatus
            {
                MasterVolume = _audioService.GetMasterVolume(),
                Mute = _audioService.GetMasterVolumeMute(),
                PowerStatus = true,
                ActiveInput = _audioService.GetDefaultAudioDevice().ToString()
            };
        }
    }
}
