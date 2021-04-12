using FluentAssertions;
using HomeCenter.Actors.Tests.Builders;
using HomeCenter.Actors.Tests.Helpers;
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
        // *[Confusion], ^[Resolved], P[PassThru], S[ShortVisit], L[LongVisit]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |           1                                                   |
        // |        |                |                       |    3                 |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |             2                   |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Any move in room should be treat as one person")]
        public async Task Count1()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms())
                .WithMotions(new Dictionary<string, string>
                {
                    { "100", Detectors.livingRoom },
                    { "500", Detectors.kitchen },
                    { "1500", Detectors.badroomDetector }
                })
                .Build();

            env.AdvanceToEnd();

            (await env.GetNumberOfPersosns(Detectors.livingRoom)).Should().Be(1);
            (await env.GetNumberOfPersosns(Detectors.kitchen)).Should().Be(1);
            (await env.GetNumberOfPersosns(Detectors.badroomDetector)).Should().Be(1);
        }

        // *[Confusion], ^[Resolved], P[PassThru], S[ShortVisit], L[LongVisit]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |           1        2                                          |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    3*       0S                  |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "When entering already occupied room we have to wait for confusion resolution")]
        public async Task Count2()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms())
                .WithMotions(new Dictionary<string, string>
                {
                    { "1/21", Detectors.kitchen },
                    { "5000", Detectors.livingRoom },
                    { "6000", Detectors.hallwayLivingRoom },
                    { "7000", Detectors.hallwayToilet }
                })
                .Build();

            // Move to confusion time
            env.AdvanceTo(TimeSpan.FromMilliseconds(7100));
            (await env.GetNumberOfPersosns(Detectors.kitchen)).Should().Be(1);
            (await env.GetNumberOfPersosns(Detectors.hallwayLivingRoom)).Should().Be(1);
            (await env.GetNumberOfPersosns(Detectors.hallwayToilet)).Should().Be(1);
            (await env.HasConfusions(Detectors.hallwayToilet)).Should().Be(true);

            env.AdvanceTo(TimeSpan.FromMilliseconds(7100) + MotionDefaults.ConfusionResolutionTime, true);
            (await env.GetNumberOfPersosns(Detectors.kitchen)).Should().Be(1);
            (await env.GetNumberOfPersosns(Detectors.hallwayLivingRoom)).Should().Be(0, "After resolving confusion have 0 persons");
            (await env.GetNumberOfPersosns(Detectors.hallwayToilet)).Should().Be(1);
            (await env.HasConfusions(Detectors.hallwayToilet)).Should().Be(false);

            env.AdvanceTo(TimeSpan.FromMilliseconds(9100) + MotionDefaults.ConfusionResolutionTime, true);
        }

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

            //var status = await env.Query<MotionStatus>(MotionServiceStatusQuery.Create());

            // status.NumberOfPersonsInHouse.Should().Be(2);
        }
    }
}