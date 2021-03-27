using FluentAssertions;
using HomeCenter.Abstractions;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.MotionService.Model;
using System;
using System.Collections.Generic;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class WorkingTimeTests : LightAutomationServiceTestsBase
    {
        [Fact(DisplayName = "During daylight should power on day lights")]
        public void WorkiTime()
        {
            var servieConfig = GetServiceBuilder().WithWorkingTime(WorkingTime.DayLight).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.kitchen },
                { 2000, Detectors.livingRoom }
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
            var servieConfig = GetServiceBuilder().WithWorkingTime(WorkingTime.DayLight).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.kitchen },
                { 2000, Detectors.livingRoom }
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
            var servieConfig = GetServiceBuilder().WithWorkingTime(WorkingTime.AfterDusk).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.kitchen },
                { 2000, Detectors.livingRoom }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.kitchen).Should().BeFalse();
            env.LampState(Detectors.livingRoom).Should().BeFalse();
        }
    }
}