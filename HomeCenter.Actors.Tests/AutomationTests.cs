using FluentAssertions;
using HomeCenter.Actors.Tests.Builders;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.MotionService.Commands;
using Microsoft.Reactive.Testing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class AutomationTests : ReactiveTest
    {
        [Fact(DisplayName = "When disabled should not turn on lights")]
        public async Task Automation1()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet }
            }).Build();

            env.SendCommand(DisableAutomationCommand.Create(Detectors.toilet));
            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
            var automationState = await env.Query<bool>(AutomationStateQuery.Create(Detectors.toilet));
            automationState.Should().BeFalse();
        }

        [Fact(DisplayName = "When re-enabled should turn on lights")]
        public void Automation2()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 2500, Detectors.toilet }
            }).Build();

            env.SendCommand(DisableAutomationCommand.Create(Detectors.toilet));
            env.SendAfterFirstMove(_ => env.SendCommand(EnableAutomationCommand.Create(Detectors.toilet)));

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.AdvanceToEnd();
            env.LampState(Detectors.toilet).Should().BeTrue();
        }
    }
}