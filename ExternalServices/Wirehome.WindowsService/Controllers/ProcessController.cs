using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Wirehome.ComponentModel.Adapters.Pc;
using Wirehome.WindowsService.Services;

namespace Wirehome.WindowsService.Controllers
{
    [Route("api/[controller]")]
    public class ProcessController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IProcessService _processService;

        public ProcessController(IHostingEnvironment hostingEnvironment, IProcessService processService)
        {
            _hostingEnvironment = hostingEnvironment;
            _processService = processService;
        }

        private string ReadProcessPath(string processName)
        {
            var configuration = Path.Combine(_hostingEnvironment.ContentRootPath, "configuration.json");

            if (!System.IO.File.Exists(configuration)) throw new Exception("Configuration file was not found");

            var jsonConfig = JObject.Parse(System.IO.File.ReadAllText(configuration));

            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonConfig["Process"].ToString());

            if (!map.ContainsKey(processName)) throw new Exception($"Process {processName} is not registred in Wirehome Winsows Service");

            return map[processName];
        }

        [HttpGet]
        public bool Get(string processName)
        {
            return _processService.IsProcessStarted(ReadProcessPath(processName));
        }
        
        [HttpPost]
        public IActionResult Post([FromBody] ProcessPost processPost)
        {
            var processPath = ReadProcessPath(processPost.ProcessName);

            if (processPost.Start)
            {
                _processService.StartProcess(processPath);
            }
            else
            {
                _processService.StopProcess(processPath);
            }

            return Ok();
        }
    }
}
