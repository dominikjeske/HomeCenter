using FluentAssertions;
using HomeCenter.Actors.Tests.Helpers;
using System.Collections.Generic;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class MoveTests : LightAutomationServiceTestsBase
    {
        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |            3                                                  |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |            1                    |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    0     |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Move on separate rooms should turn on light")]
        public void Move1()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.kitchen },
                { 2000, Detectors.livingRoom }
            }).Build();

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeTrue();
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.LampState(Detectors.livingRoom).Should().BeTrue();
        }
    }
}