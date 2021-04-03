using FluentAssertions;
using HomeCenter.Abstractions;
using HomeCenter.Actors.Tests.Builders;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class WorkingTimeTests : ReactiveTest
    {
        [Fact(DisplayName = "During daylight should power on day lights")]
        public void WorkiTime()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms()
                                                                                               .WithWorkingTime(WorkingTime.DayLight))
                .WithMotions(new Dictionary<string, string>
            {
                { "500", Detectors.toilet },
                { "1500", Detectors.kitchen },
                { "2000", Detectors.livingRoom }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeTrue();
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.LampState(Detectors.livingRoom).Should().BeTrue();
        }

        [Fact(DisplayName = "After dusk should not power on day light")]
        public void WorkingTime_AfterDusk_ShouldNotPowerOnDayLight()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms()
                                                                                               .WithWorkingTime(WorkingTime.DayLight))
                .WithMotions(new Dictionary<string, string>
            {
                { "500", Detectors.toilet },
                { "1500", Detectors.kitchen },
                { "2000", Detectors.livingRoom }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(21));

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.kitchen).Should().BeFalse();
            env.LampState(Detectors.livingRoom).Should().BeFalse();
        }

        [Fact(DisplayName = "During day light should not power on night light")]
        public void WorkingTime_DuringDaylight_ShuldNotPowerOnNightLight()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms().WithWorkingTime(WorkingTime.AfterDusk))
                .WithMotions(new Dictionary<string, string>
            {
                { "500", Detectors.toilet },
                { "1500", Detectors.kitchen },
                { "2000", Detectors.livingRoom }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.kitchen).Should().BeFalse();
            env.LampState(Detectors.livingRoom).Should().BeFalse();
        }
    }
}