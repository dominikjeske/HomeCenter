using HomeCenter.WindowsService.Core.Interfaces;
using HomeCenter.WindowsService.Core.Interop.Enum;
using HomeCenter.Adapters.PC.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.WindowsService.Controllers
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
            var devices = _audioService.GetAudioDevices(AudioDeviceKind.Playback, AudioDeviceState.Active);
            return devices.Select(x => new AudioDeviceInfo { Id = x.Id, Name = x.ToString() }).ToList();
        }

        [HttpPost]
        public IActionResult Post([FromBody] InputSourcePost inputSource)
        {
            var device = _audioService.GetAudioDevices(AudioDeviceKind.Playback, AudioDeviceState.Active).FirstOrDefault(x => x.ToString() == inputSource.Input);
            if (device == null) throw new System.Exception($"Device {inputSource.Input} was not found oncomputer");

            _audioService.SetDefaultAudioDevice(device);
            return Ok();
        }
    }
}