using HomeCenter.Adapters.PC.Model;
using HomeCenter.WindowsService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HomeCenter.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class VolumeController : Controller
    {
        private readonly IAudioService _audioService;

        public VolumeController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet]
        public float Get()
        {
            return _audioService.GetMasterVolume();
        }

        [HttpPost]
        public IActionResult Post([FromBody] VolumePost volume)
        {
            _audioService.SetMasterVolume((float)volume.Volume);
            return Ok();
        }
    }
}