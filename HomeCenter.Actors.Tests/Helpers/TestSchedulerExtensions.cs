using System;
using System.Linq;
using HomeCenter.Services.MotionService.Tests;
using Microsoft.Reactive.Testing;

namespace HomeCenter.Actors.Tests.Helpers
{
    public static class TestExtensions
    {
        private static int _deltaMiliseconds = 500;

        public static void AdvanceToEnd<T>(this TestScheduler scheduler, ITestableObservable<T> events)
        {
            scheduler.AdvanceTo(events.Messages.Max(x => x.Time));
        }

        public static void AdvanceJustAfterEnd<T>(this TestScheduler scheduler, ITestableObservable<T> events, int timeAfter = 500)
        {
            scheduler.AdvanceTo(events.Messages.Max(x => x.Time) + Time.Tics(timeAfter));
        }

        public static void AdvanceJustAfter(this TestScheduler scheduler, int time)
        {
            scheduler.AdvanceTo(TimeSpan.FromMilliseconds(time).JustAfter().Ticks);
        }

        public static void AdvanceJustAfter(this TestScheduler scheduler, TimeSpan time)
        {
            scheduler.AdvanceTo(time.JustAfter().Ticks);
        }

        public static void AdvanceJustBefore(this TestScheduler scheduler, TimeSpan time)
        {
            scheduler.AdvanceTo(time - TimeSpan.FromMilliseconds(_deltaMiliseconds));
        }

        public static void AdvanceTo(this TestScheduler scheduler, TimeSpan time)
        {
            scheduler.AdvanceTo(time.Ticks);
        }

        public static void AdvanceAfterElement<T>(this TestScheduler scheduler, ITestableObservable<T> events, int elementIndex, int timeAfter = 500)
        {
            scheduler.AdvanceTo(events.Messages.ElementAt(elementIndex).Time + Time.Tics(timeAfter));
        }

        public static TimeSpan JustAfter(this TimeSpan span, int timeAfter = 100) => span.Add(TimeSpan.FromMilliseconds(timeAfter));
    }
}