using HomeCenter.Services.Configuration;
using Windows.ApplicationModel.Background;

namespace HomeCenter.Controller
{
    public sealed class StartupTask : IBackgroundTask
    {
        private Model.Core.Controller _controller;
        private IBootstrapper _bootstrapper;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            try
            {
                _bootstrapper = new RaspberryBootstrapper();
                _controller = await _bootstrapper.BuildController().ConfigureAwait(false);
            }
            catch (System.Exception)
            {
                deferral.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _controller.Dispose();
            _bootstrapper.Dispose();
        }
    }
}