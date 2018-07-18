using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Wirehome.WindowsService.Core;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class DisplayController : Controller
    {
        private readonly IDisplayService _displayService;

        public DisplayController(IDisplayService displayService)
        {
            _displayService = displayService;
        }

        [HttpGet]
        public IEnumerable<IDisplay> Get()
        {
            return _displayService.GetActiveMonitors();
        }
        
        [HttpPost]
        public IActionResult Post(DisplayMode displayMode)
        {
            _displayService.SetDisplayMode(displayMode);

            return Ok();
        }
    }
}
