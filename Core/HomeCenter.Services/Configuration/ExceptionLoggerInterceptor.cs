using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.Configuration
{
    public class ExceptionLoggerInterceptor : IAsyncInterceptor
    {
        private readonly ILogger<ExceptionLoggerInterceptor> _logger;

        public ExceptionLoggerInterceptor(ILogger<ExceptionLoggerInterceptor> logger)
        {
            _logger = logger;
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
                var task = (Task<TResult>)invocation.ReturnValue;
                var result = await task;
                return result;
            }
            catch (Exception e)
            {
                HandleException(e, invocation);
                throw;
            }
        }

        private async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
                var task = (Task)invocation.ReturnValue;
                await task;
            }
            catch (Exception e)
            {
                HandleException(e, invocation);
                throw;
            }
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
            }
            catch (Exception e)
            {
                HandleException(e, invocation);
                throw;
            }
        }

        private void HandleException(Exception e, IInvocation invocation)
        {
            _logger.LogError(e, $"Unhanded exception in {invocation.Method.DeclaringType.Name}.{invocation.Method.Name}");
        }
    }
}