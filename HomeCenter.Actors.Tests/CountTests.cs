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
        // |        |           1          2                                        |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |     3       4                   |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Move pass rooms without confusion")]
        public async Task Count3()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms())
            .WithMotions(new Dictionary<string, string>
            {
                { "100", Detectors.livingRoom },
                { "1500", Detectors.hallwayLivingRoom },
                { "3500", Detectors.hallwayToilet },
                { "4500", Detectors.kitchen }
            })
            .Build();

            env.AdvanceTo(TimeSpan.FromMilliseconds(1500));
            (await env.GetNumberOfPersosns(Detectors.hallwayLivingRoom)).Should().Be(1);

            env.AdvanceTo(TimeSpan.FromMilliseconds(4000));
            (await env.GetNumberOfPersosns(Detectors.hallwayLivingRoom)).Should().Be(0, "When no confusion we are sure quickly");
            (await env.GetNumberOfPersosns(Detectors.hallwayToilet)).Should().Be(1);

            env.AdvanceTo(TimeSpan.FromMilliseconds(5000));
            (await env.GetNumberOfPersosns(Detectors.hallwayToilet)).Should().Be(0, "When no confusion we are sure quickly");
            (await env.GetNumberOfPersosns(Detectors.kitchen)).Should().Be(1);
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
        // |        |                |            |    3*       0L                  |
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
            // Confusion resolution for 2->3
            env.AdvanceTo(TimeSpan.FromMilliseconds(7100) + MotionDefaults.ConfusionResolutionTime, true);
            (await env.GetNumberOfPersosns(Detectors.kitchen)).Should().Be(1);
            (await env.GetNumberOfPersosns(Detectors.hallwayLivingRoom)).Should().Be(0, "After resolving confusion have 0 persons");
            (await env.GetNumberOfPersosns(Detectors.hallwayToilet)).Should().Be(1);
            (await env.HasConfusions(Detectors.hallwayToilet)).Should().Be(false);
            // Confusion resolution for 3->0
            env.AdvanceTo(TimeSpan.FromMilliseconds(9100) + MotionDefaults.ConfusionResolutionTime, true);
            (await env.GetNumberOfPersosns(Detectors.kitchen)).Should().Be(2, "After resolving confusion have 2 persons");
            (await env.GetNumberOfPersosns(Detectors.hallwayLivingRoom)).Should().Be(0);
            (await env.GetNumberOfPersosns(Detectors.hallwayToilet)).Should().Be(0);
            (await env.HasConfusions(Detectors.hallwayToilet)).Should().Be(false);
            (await env.HasConfusions(Detectors.kitchen)).Should().Be(false);
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
        // |        |                |            |     1        0L                 |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Exit from room after long stay")]
        public async Task Count4()
        {
            using var env = EnviromentBuilder.Create(s => s.WithDefaultRooms())
                .WithMotions(new Dictionary<string, string>
                {
                    { "1/21", Detectors.kitchen },
                    { "22000", Detectors.hallwayToilet },
                })
                .Build();

            env.AdvanceTo(TimeSpan.FromMilliseconds(24000), justAfter: true);
            (await env.GetNumberOfPersosns(Detectors.kitchen)).Should().Be(0, "Event when we have long move when no confusion we are sure");
            (await env.GetNumberOfPersosns(Detectors.hallwayToilet)).Should().Be(1);
        }
    }
}