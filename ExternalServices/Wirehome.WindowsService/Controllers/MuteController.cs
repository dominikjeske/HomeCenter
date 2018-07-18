using Microsoft.AspNetCore.Mvc;
using Wirehome.ComponentModel.Adapters.Pc;
using Wirehome.WindowsService.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class MuteController : Controller
    {
        private readonly IAudioService _audioService;

        public MuteController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet]
        public bool Get()
        {
            return _audioService.GetMasterVolumeMute();
        }
        
        [HttpPost]
        public IActionResult Post([FromBody] MutePost value)
        {
            _audioService.SetMasterVolumeMute(value.Mute);

            return Ok();
        }
    }
}
