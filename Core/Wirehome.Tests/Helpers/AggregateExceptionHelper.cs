using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Tests
{
    public static class AggregateExceptionHelper
    {
        public static void AssertAggregateException(this AggregateException ae, Type typeOfInner, string message = null)
        {
            while(ae.InnerException.GetType() == typeof(AggregateException))
            {
                ae = ae.InnerException as AggregateException;
            }

            Assert.AreEqual(ae.InnerException.GetType(), typeOfInner);
            if (message != null)
            {
                Assert.AreEqual(ae.InnerException.Message, message);
            }
        }

        public static void AssertInnerException<T>(Action a, string message = null)
        {
            try
            {
                a();
            }
            catch (AggregateException ex)
            {
                ex.AssertAggregateException(typeof(T), message);
                return;
            }
            catch (Exception e)
            {
                Assert.Fail($"Excepted {typeof(T).Name} Exception but {e.GetType().Name} Exception was thrown with message: {e.Message}");
                return;
            }

            Assert.Fail($"Excepted {typeof(T).Name} Exception but no Exception was thrown");
        }

        public static void AssertInnerException<T>(Task task, string message = null)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                ex.AssertAggregateException(typeof(T), message);
                return;
            }
            catch (Exception e)
            {
                Assert.Fail($"Excepted {typeof(T).Name} Exception but {e.GetType().Name} Exception was thrown with message: {e.Message}");
                return;
            }

            Assert.Fail($"Excepted {typeof(T).Name} Exception but no Exception was thrown");
        }

        public static void AssertInnerException<T>(IObservable<object> subscription, string message = null)
        {
            try
            {
                subscription.ToTask().Wait();
            }
            catch (AggregateException ex)
            {
                ex.AssertAggregateException(typeof(T), message);
                return;
            }
            catch( Exception e)
            {
                Assert.Fail($"Excepted {typeof(T).Name} Exception but {e.GetType().Name} Exception was thrown with message: {e.Message}");
                return;
            }

            Assert.Fail($"Excepted {typeof(T).Name} Exception but no Exception was thrown");
        }
    }
}
