using Microsoft.AspNetCore.Mvc;
using Wirehome.WindowsService.Core;
using System.Linq;
using System.Collections.Generic;
using Wirehome.ComponentModel.Adapters.Pc;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class InputSourceController : Controller
    {
        private readonly IAudioService _audioService;

        public InputSourceController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        [HttpGet]
        public IEnumerable<AudioDeviceInfo> Get()
        {
            var devices = _audioService.GetAudioDevices(Interop.AudioDeviceKind.Playback, Interop.AudioDeviceState.Active);
            return devices.Select(x => new AudioDeviceInfo { Id = x.Id, Name = x.ToString() }).ToList();
        }

        [HttpPost]
        public IActionResult Post([FromBody] InputSourcePost inputSource)
        {
            var device = _audioService.GetAudioDevices(Interop.AudioDeviceKind.Playback, Interop.AudioDeviceState.Active).FirstOrDefault(x => x.ToString() == inputSource.Input);
            if (device == null) throw new System.Exception($"Device {inputSource.Input} was not found oncomputer");

            _audioService.SetDefaultAudioDevice(device);
            return Ok();
        }
    }
}
