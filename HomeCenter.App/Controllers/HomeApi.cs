using HomeCenter.Abstractions;
using HomeCenter.Adapters.Common;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Events.Device;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace HomeCenter.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeApi : ControllerBase
    {
        private readonly IMessageBroker _messageBroker;

        public HomeApi(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        [HttpGet]
        public void TurnOn(string uid)
        {
            Command cmd = new TurnOnCommand();

            //cmd.SetProperty(MessageProperties.PinNumber, pinNumber.Value);
            _messageBroker.Send(cmd, uid);
        }
    }
}