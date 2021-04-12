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
    public class ConfusionTests : ReactiveTest
    {
        // *[Confusion], ^[Resolved], P[PassThru], S[ShortVisit], L[LongVisit]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |            4      5                                           |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    2        3                   |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |     1    |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "First not confused moving path should not be source of confusion for next path")]
        public async Task Confusion()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms())
                .WithMotions(new Dictionary<string, string>
            {
                 // First path
                { "500", Detectors.toilet },
                { "1500", Detectors.hallwayToilet },
                { "2000", Detectors.kitchen },
                // Second path
                { "2500", Detectors.livingRoom },
                { "3000", Detectors.hallwayLivingRoom }
            }).Build();

            env.AdvanceToEnd();

            //var status = await env.Query<MotionStatus>(MotionServiceStatusQuery.Create());
            //status.NumberOfConfusions.Should().Be(0);
        }
    }
}