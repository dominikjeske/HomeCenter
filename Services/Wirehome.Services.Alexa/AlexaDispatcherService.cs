

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wirehome.Alexa.Model.Common;
using Wirehome.Alexa.Model.Discovery;
using Wirehome.Core.Exceptions;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Extensions
{
    public class AlexaDispatcherService : IAlexaDispatcherService, IHttpContextPipelineHandler
    {
        private const string SERVICE_URI = "/alexa";

        private readonly IHttpServerService _httpServer;
        private readonly IAreaRegistryService _areService;
        private readonly ISettingsService _settingService;
        private readonly IComponentRegistryService _componentService;
        private readonly ILogger _log;
        
        private Dictionary<string, IEnumerable<IComponent>> _registredDevices = new Dictionary<string, IEnumerable<IComponent>>();
        
        public AlexaDispatcherService(IHttpServerService httpServer, IAreaRegistryService areService, ISettingsService settingService, 
            IComponentRegistryService componentService, ILogService logService)
        {
            _httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));
            _areService = areService ?? throw new ArgumentNullException(nameof(areService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));

            _log = logService.CreatePublisher(nameof(AlexaDispatcherService));
        }

        public Task Initialize()
        {
            _httpServer.AddRequestHandler(this);
            return Task.CompletedTask;
        }

        public Task ProcessResponseAsync(HttpContextPipelineHandlerContext context) => Task.CompletedTask;

        public Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
        {
            if (!context.HttpContext.Request.Uri.StartsWith(SERVICE_URI) || !context.HttpContext.Request.Method.Equals(HttpMethod.Post)) return Task.CompletedTask;
                        
            var request = JsonConvert.DeserializeObject<SmartHomeRequest>(new StreamReader(context.HttpContext.Request.Body).ReadToEnd());
            var response = DispatchHttpRequest(request);
            
            context.HttpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
   
            return Task.CompletedTask;
        }

        public void RegisterDevice(string friendlyName, IEnumerable<IComponent> devices)
        {
            if (_registredDevices.ContainsKey(friendlyName))
            {
                throw new Exception($"Friendly name '{friendlyName}' is already in use");
            }

            _registredDevices.Add(friendlyName, devices);
        }

        private object DispatchHttpRequest(SmartHomeRequest request)
        {
            var requestName = request.Directive.Header.Name;

            if (requestName.IndexOf("Discover") > -1)
            {
                return PrepareDicsoverMessage();
            }
            else if (requestName.IndexOf("TurnOn") > -1)
            {
               return PrepareInvokeMessage(request);
            }

            return null;
        }

        private object PrepareDicsoverMessage()
        {
            var devices = new List<AlexaDevice>();

            foreach (var area in _areService.GetAreas())
            {
                var areaName = area.Settings?.Caption;
                var areaComponents = area.GetComponents();

                devices.AddRange(GetDevicesFromArea(areaName, areaComponents));
            }

            devices.AddRange(GetExplisitRegiterDevices());

            return DiscoverResponse.GenerateResponse(devices);
        }

        private List<AlexaDevice> GetDevicesFromArea(string areaName, IList<IComponent> areaComponents)
        {
            var devices = new List<AlexaDevice>();

            foreach (var compoment in areaComponents)
            {
                var actions = GetSupportedStates(compoment);
                var componentId = GetCompatibileComponentID(compoment);
                var friendlyName = GetFriendlyName(areaName, compoment);

                if (actions.Count == 0 || string.IsNullOrWhiteSpace(friendlyName))
                {
                    continue;
                }

                devices.Add(new AlexaDevice()
                {
                    //Capabilities = actions,
                    Uid = componentId,
                    FriendlyName = friendlyName,
                    Room = areaName
                });
            }

            return devices;
        }
        
        private List<AlexaDevice> GetExplisitRegiterDevices()
        {
            var devices = new List<AlexaDevice>();

            foreach (var friendlyName in _registredDevices.Keys)
            {
                var connectedDevices = _registredDevices[friendlyName];

                var actions = GetSupportedStates(connectedDevices.FirstOrDefault());
                var componentId = $"Composite_{friendlyName.Replace(" ", "_")}";

                if (actions.Count == 0 || string.IsNullOrWhiteSpace(friendlyName))
                {
                    continue;
                }

                devices.Add(new AlexaDevice()
                {
                    //Capabilities = actions,
                    Uid = componentId,
                    FriendlyName = friendlyName,
                    Room = "None"
                });
            }

            return devices;
        }
        
        private string GetFriendlyName(string areaName, IComponent compoment)
        {
            string friendlyName = string.Empty;

            var componentSetting = _settingService.GetSettings<ComponentSettings>(compoment.Id);

            if (componentSetting != null)
            {
                var componentName = componentSetting.Caption;
                if (string.IsNullOrWhiteSpace(componentName) || string.IsNullOrWhiteSpace(areaName))
                {
                    return compoment.Id.Replace(".", " ");
                }
                else
                {
                    friendlyName = $"{areaName} {componentName}";
                }
            }

            return friendlyName;
        }

        private static string GetCompatibileComponentID(IComponent compoment)
        {
            return compoment.Id.Replace(".", "_");
        }

        private List<string> GetSupportedStates(IComponent component)
        {
            var actions = new List<string>();

            if(component.GetFeatures().Supports<PowerStateFeature>())
            {
                actions.Add("turnOn");
                actions.Add("turnOff");
            }

            return actions;
        }

        private object PrepareInvokeMessage(SmartHomeRequest request)
        {
            try
            {
                var componentID = request.Directive.Endpoint.EndpointId;

                if (string.IsNullOrWhiteSpace(componentID))
                {
                    throw new NotFoundException();
                }

                if (componentID.IndexOf("Composite") > -1)
                {
                    // Cut component prefix
                    componentID = componentID.Substring(10, componentID.Length - 10);
                    componentID = componentID.Replace("_", " ");

                    if (!_registredDevices.ContainsKey(componentID))
                    {
                        throw new NotFoundException();
                    }

                    foreach (var device in _registredDevices[componentID])
                    {
                        RunComponentCommand(request.Directive.Header.Name, device.Id, true);
                    }
                }
                else
                {
                    componentID = componentID.Replace("_", ".");

                    RunComponentCommand(request.Directive.Header.Name, componentID);
                }
            }
            catch (NotFoundException)
            {
                
            }
            catch (StateAlreadySetException)
            {
               
            }

            // TODO prepare response
            return null;
        }

        private void RunComponentCommand(string command, string componentID, bool ignoreCurrentStateCheck = false)
        {
            var component = _componentService.GetComponent(componentID);

            // TODO invoke command
        }
    }
}
