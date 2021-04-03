using FluentAssertions;
using HomeCenter.Actors.Tests.Builders;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class CountTests : ReactiveTest
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
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms())
                .WithMotions(new Dictionary<string, string> //Short moves should not be included
                {
                    { "100/15", Detectors.livingRoom },
                    { "101/15", Detectors.toilet },
                    { "500", Detectors.kitchen },
                    { "1500", Detectors.hallwayToilet }
                })
                .Build();

            env.AdvanceToEnd();

            var status = await env.Query<MotionStatus>(MotionServiceStatusQuery.Create());

            status.NumberOfPersonsInHouse.Should().Be(2);
        }
    }
}