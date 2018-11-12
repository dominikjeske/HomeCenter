using AutoMapper;
using CSharpFunctionalExtensions;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Services.Configuration;
using HomeCenter.Services.Roslyn;
using HomeCenter.Utils;
using Microsoft.Extensions.Logging;
using Proto.Remote;
using Quartz;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HomeCenter.Services.Controllers
{
    [ProxyCodeGenerator]
    public abstract class Controller : DeviceActor
    {
        private readonly IRoslynCompilerService _roslynCompilerService;
        private readonly IControllerOptions _controllerOptions;
        private readonly IConfigurationService _configurationService;
        private readonly IResourceLocatorService _resourceLocatorService;
        private readonly IMapper _mapper;

        private HomeCenterConfiguration _homeConfiguration;

        protected Controller(IMapper mapper, IRoslynCompilerService roslynCompilerService, IResourceLocatorService resourceLocatorService, 
            IConfigurationService configurationService, IControllerOptions controllerOptions)
        {
            _roslynCompilerService = roslynCompilerService;
            _controllerOptions = controllerOptions;
            _configurationService = configurationService;
            _resourceLocatorService = resourceLocatorService;
            _mapper = mapper;
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            //TODO Move to bootstrapper?
            LoadDynamicAdapters(_controllerOptions.AdapterMode);

            await LoadCalendars().ConfigureAwait(false);
            InitializeConfiguration();
            await RunScheduler().ConfigureAwait(false);

            Remote.Start(_controllerOptions.RemoteActorAddress ?? "127.0.0.1", _controllerOptions.RemoteActorPort ?? 8000);
        }

        private async Task LoadCalendars()
        {
            foreach (var calendarType in AssemblyHelper.GetAllTypes<ICalendar>())
            {
                var cal = calendarType.CreateInstance<ICalendar>();
                await Scheduler.AddCalendar(calendarType.Name, cal, false, false).ConfigureAwait(false);
            }
        }

        private Task RunScheduler() => Scheduler.Start(_disposables.Token);

        private void LoadDynamicAdapters(AdapterMode adapterMode)
        {
            Logger.LogInformation($"Loading adapters in mode: {adapterMode}");

            if (adapterMode == AdapterMode.Compiled)
            {
                var result = _roslynCompilerService.CompileAssemblies(_resourceLocatorService.GetRepositoyLocation());
                var veryfy = Result.Combine(result.ToArray());
                if (veryfy.IsFailure) throw new CompilationException($"Error while compiling adapters: {veryfy.Error}");

                foreach (var adapter in result)
                {
                    Assembly.LoadFrom(adapter.Value);
                }
            }
            else
            {
                Logger.LogInformation($"Using only build in adapters");
            }
        }

        private void InitializeConfiguration()
        {
            _homeConfiguration = _configurationService.ReadConfiguration(_controllerOptions.AdapterMode);

        }

        protected HomeCenterConfiguration Handle(StateQuery state)
        {
            return _homeConfiguration;
        }
    }
}