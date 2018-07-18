using Quartz.Spi;
using System;
using Quartz;
using System.Threading.Tasks;
using Wirehome.Core.Services.DependencyInjection;

namespace Wirehome.Core.Services.Quartz
{
    internal class JobWrapper : IJob
    {
        private readonly TriggerFiredBundle _bundle;
        private readonly IContainer _container;

        public JobWrapper(TriggerFiredBundle bundle, IContainer container)
        {
            _bundle = bundle;
            _container = container;
        }

        protected IJob RunningJob { get; private set; }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                RunningJob = _container.GetInstance(_bundle.JobDetail.JobType) as IJob;
                return RunningJob.Execute(context);
            }
            catch (JobExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new JobExecutionException($"Failed to execute Job '{_bundle.JobDetail.Key}' of type '{_bundle.JobDetail.JobType}'", ex);
            }
            finally
            {
                RunningJob = null;
            }
        }
    }
}
