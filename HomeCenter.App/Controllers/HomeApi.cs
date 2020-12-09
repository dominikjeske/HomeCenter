using HomeCenter.Adapters.Common;
using Microsoft.AspNetCore.Mvc;

namespace HomeCenter.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeApi : ControllerBase
    {
        public HomeApi(CCToolsAdapterProxy cCToolsAdapterProxy)
        {

        }

        [HttpGet]
        public string Get()
        {
            return "Test";
        }
    }
}