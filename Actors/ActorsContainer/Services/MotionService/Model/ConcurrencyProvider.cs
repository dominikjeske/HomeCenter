﻿using System.Reactive.Concurrency;

namespace HomeCenter.Services.MotionService.Model
{
    public class ConcurrencyProvider : IConcurrencyProvider
    {
        public ConcurrencyProvider()
        {
            Scheduler = DefaultScheduler.Instance;
            Task = TaskPoolScheduler.Default;
            Thread = NewThreadScheduler.Default;
        }

        public IScheduler Scheduler { get; }
        public IScheduler Task { get; }
        public IScheduler Thread { get; }
        public IScheduler Dispatcher { get; }
    }
}