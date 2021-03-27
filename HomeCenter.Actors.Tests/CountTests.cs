using FluentAssertions;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class CountTests : LightAutomationServiceTestsBase
    {
        // TODO - Count should not decrease  when there is no exit from house
        // *[Confusion], ^[Resolved], P[PassThru], S[ShortVisit], L[LongVisit]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |           1S                                                  |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    4        3                   |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    2S    |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "When moving around we should have count without short moves")]
        public async Task Count()
        {
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(MotionDefaults.ConfusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithRepeatedMotions(Detectors.livingRoom, TimeSpan.FromSeconds(15))
                                                                                        .WithRepeatedMotions(Detectors.toilet, TimeSpan.FromSeconds(15))
                                                                                        .WithMotions(new Dictionary<int, string> //Short moves should not be included
                                                                                        {
                                                                                            { 500, Detectors.kitchen },
                                                                                            { 1500, Detectors.hallwayToilet }
                                                                                        })
                                                                                        .Build();

            env.AdvanceToEnd();

            var status = await env.Query<MotionStatus>(MotionServiceStatusQuery.Create());

            status.NumberOfPersonsInHouse.Should().Be(2);
        }
    }
}