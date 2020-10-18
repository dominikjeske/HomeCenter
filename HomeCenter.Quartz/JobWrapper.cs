using Quartz;
using Quartz.Spi;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Quartz
{
    internal class JobWrapper : IJob
    {
        private readonly TriggerFiredBundle _bundle;
        private readonly IServiceProvider _serviceProvider;

        public JobWrapper(TriggerFiredBundle bundle, IServiceProvider serviceProvider)
        {
            _bundle = bundle;
            _serviceProvider = serviceProvider;
        }

        protected IJob RunningJob { get; private set; }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                RunningJob = _serviceProvider.GetService(_bundle.JobDetail.JobType) as IJob;
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