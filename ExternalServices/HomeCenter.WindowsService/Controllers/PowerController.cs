using HomeCenter.WindowsService.Core.Interfaces;
using HomeCenter.WindowsService.Core.Power;
using HomeCenter.Adapters.PC.Model;
using Microsoft.AspNetCore.Mvc;

namespace HomeCenter.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class PowerController : Controller
    {
        private readonly IPowerService _powerService;

        public PowerController(IPowerService powerService)
        {
            _powerService = powerService;
        }

        [HttpGet]
        public bool Get()
        {
            return true;
        }

        [HttpPost]
        public IActionResult Post([FromBody] PowerPost value)
        {
            _powerService.SetPowerMode((ComputerPowerState)value.State);
            return Ok();
        }
    }
}