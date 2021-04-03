using FluentAssertions;
using HomeCenter.Actors.Tests.Builders;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.MotionService.Commands;
using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class TimeoutTests : ReactiveTest
    {
        // *[Confusion], ^[Resolved], P[PassThru], S[ShortVisit], L[LongVisit]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                                                               |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |             0P                  |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Time out after short move")]
        public void Timeout1()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(9);
      
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms()
                                                                                 .WithRoomConfig(Detectors.kitchen, r => r.WithTimeout(timeout)))
                .WithRepeatedMotions(Detectors.kitchen, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchen).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + timeout);
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.AdvanceTo(moveTime + timeout, true);
            env.LampState(Detectors.kitchen).Should().BeFalse();
        }

        // *[Confusion], ^[Resolved], P[PassThru], S[ShortVisit], L[LongVisit]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                                                               |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |             0S                  |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "After short visit should be longer")]
        public void Timeout2()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(15);


            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms().WithRoomConfig(Detectors.kitchen, r => r.WithTimeout(timeout)))
                .WithRepeatedMotions(Detectors.kitchen, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchen).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + 2 * timeout);
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.AdvanceTo(moveTime + 2 * timeout, true);
            env.LampState(Detectors.kitchen).Should().BeFalse();
        }

        // *[Confusion], ^[Resolved], P[PassThru], S[ShortVisit], L[LongVisit]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                                                               |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |             0L                  |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "After long visit should be longer")]
        public void Timeout3()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(75);

            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms().WithRoomConfig(Detectors.kitchen, r => r.WithTimeout(timeout)))
                .WithRepeatedMotions(Detectors.kitchen, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchen).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + 3 * timeout);
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.AdvanceTo(moveTime + 3 * timeout, true);
            env.LampState(Detectors.kitchen).Should().BeFalse();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                                                               |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |            0,1                  |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Move turn on after turn off should increase timeout")]
        public async Task Timeout4()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms())
                .WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchen },
                { 12000, Detectors.kitchen },
            }).Build();

            env.AdvanceToEnd();
            var status = await env.Query<bool>(AutomationStateQuery.Create(Detectors.kitchen));
            status.Should().BeTrue();
        }
    }
}