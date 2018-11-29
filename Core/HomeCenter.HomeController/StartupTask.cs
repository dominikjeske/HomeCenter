using SimpleInjector;
using System;
using System.Net.Http;
using Windows.ApplicationModel.Background;

namespace HomeCenter.HomeController
{
    public sealed class StartupTask : IBackgroundTask
    {
        private RaspberryBootstrapper _bootstrapper;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            try
            {
                var container = new Container();
                _bootstrapper = new RaspberryBootstrapper(container);
                await _bootstrapper.BuildController().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _bootstrapper.LogException(e.ToString());
                deferral.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _bootstrapper.LogException($"Application was canceled because of '{reason.ToString()}'");
            _bootstrapper.Dispose();
        }
    }
}