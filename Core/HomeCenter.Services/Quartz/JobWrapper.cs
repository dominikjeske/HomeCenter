using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Core.Quartz
{
    internal class JobWrapper : IJob
    {
        private readonly TriggerFiredBundle _bundle;
        private readonly Container _container;

        public JobWrapper(TriggerFiredBundle bundle, Container container)
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