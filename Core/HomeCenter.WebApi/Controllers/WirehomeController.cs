using HomeCenter.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Proto;

namespace HomeCenter.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class WirehomeController : ControllerBase
    {
        private readonly RootContext _context = new RootContext();
        private readonly IOptions<WireHomeConfigSection> _appSettings;

        public WirehomeController(IOptions<WireHomeConfigSection> app)
        {
            _appSettings = app;
        }

        [HttpPost]
        public void Post([FromBody] ProtoCommand command)
        {
            var address = new PID(_appSettings.Value.Address, command.Address);
            _context.Send(address, command);
        }
    }
}