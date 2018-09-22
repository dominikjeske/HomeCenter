using Microsoft.AspNetCore.Mvc;
using HomeCenter.Model.Adapters.Pc;
using HomeCenter.WindowsService.Core;

namespace HomeCenter.WindowsService.Controllers
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
