using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using HomeCenter.Core.Interface.Native;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Core.Services.Logging;
using HomeCenter.Model.Core;
using HomeCenter.Raspberry;

namespace HomeCenter.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        private HomeCenterController _HomeCenterController;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            try
            {
                _HomeCenterController = new HomeCenterController(GetControllerOptions());
                await _HomeCenterController.Initialize().ConfigureAwait(false);
            }
            catch (System.Exception)
            {
                deferral.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _HomeCenterController.Dispose();
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