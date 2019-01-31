using Proto;
using SimpleInjector;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    public class ControllerBuilder
    {
        private string _configuration;
        private string _repositoryPath;
        private readonly Container _container = new Container();

        public ControllerBuilder WithConfiguration(string configuration)
        {
            _configuration = configuration;
            return this;
        }

        public ControllerBuilder WithAdapterRepositoryPath(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
            return this;
        }

        public async Task<(PID controller, Container container)> BuildAndRun()
        {
            var bootstrapper = new MockBootstrapper(_container, _repositoryPath, _configuration);
            var controller = await bootstrapper.BuildController().ConfigureAwait(false);
            return (controller, bootstrapper.Container);
        }
    }
}