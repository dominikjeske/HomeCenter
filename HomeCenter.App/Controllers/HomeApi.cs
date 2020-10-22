using Microsoft.AspNetCore.Mvc;

namespace HomeCenter.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeApi : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Test";
        }
    }
}