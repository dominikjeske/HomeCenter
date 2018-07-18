using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;
using Wirehome.Model.Core;
using Wirehome.Raspberry;

namespace Wirehome.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        private WirehomeController _WirehomeController;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            try
            {
                _WirehomeController = new WirehomeController(GetControllerOptions());
                await _WirehomeController.Initialize().ConfigureAwait(false);
            }
            catch (System.Exception)
            {
                deferral.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _WirehomeController.Dispose();
        }

        private ControllerOptions GetControllerOptions() => new ControllerOptions
        {
            NativeServicesRegistration = RegisterRaspberryServices,
            AdapterMode = AdapterMode.Embedded,
            // TODO change this to dynamic service load like adapters
            Loggers = new List<ILogAdapter> { new RaspberryLoggingService() }
        };

        private void RegisterRaspberryServices(IContainer container)
        {
            container.RegisterSingleton<INativeGpioController, RaspberryGpioController>();
            container.RegisterSingleton<INativeI2cBus, RaspberryI2cBus>();
            container.RegisterSingleton<INativeSerialDevice, RaspberrySerialDevice>();
            container.RegisterSingleton<INativeSoundPlayer, RaspberrySoundPlayer>();
            container.RegisterSingleton<INativeStorage, RaspberryStorage>();
            container.RegisterSingleton<INativeTimerSerice, RaspberryTimerSerice>();
        }
    }
}