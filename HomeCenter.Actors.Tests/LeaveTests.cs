using FluentAssertions;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.MotionService.Model;
using System;
using System.Collections.Generic;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    public class LeaveTests : LightAutomationServiceTestsBase
    {
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
        // |        |                |            |    1                            |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    0     |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Leave from one people room with no confusion should turn off light immediately")]
        public void Leave1()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.hallwayToilet }
            }).Build();

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                     3                                         |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    2*       1,4^                |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    0     |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Leave from one people room with confusion should turn off after resolving confusion")]
        public void Leave2()
        {
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(MotionDefaults.ConfusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1000, Detectors.kitchen },
                { 1500, Detectors.hallwayToilet },
                { 2000, Detectors.hallwayLivingRoom },
                { 3000, Detectors.kitchen },
            }).Build();

            env.AdvanceToIndex(2, MotionDefaults.ConfusionResolutionTime, true);

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.kitchen).Should().BeTrue();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                   1                      0                    |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |    2*    |                      |
        // |        |                |            |                                 |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Leave from separate rooms into one should turn off light in both")]
        public void Leave3()
        {
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(MotionDefaults.ConfusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.badroomDetector },
                { 1000, Detectors.hallwayLivingRoom },
                { 1500, Detectors.hallwayToilet }
            }).Build();

            env.AdvanceToIndex(2, MotionDefaults.ConfusionResolutionTime, true);

            env.LampState(Detectors.badroomDetector).Should().BeFalse();
            env.LampState(Detectors.hallwayLivingRoom).Should().BeFalse();
            env.LampState(Detectors.hallwayToilet).Should().BeTrue();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                   1                                           |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |    4^      |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    3*                           |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    2     |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Leave to other room should speed up resolution in neighbor")]
        public void Leave4()
        {
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(MotionDefaults.ConfusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.hallwayLivingRoom },
                { 1000, Detectors.toilet },
                { 1500, Detectors.hallwayToilet },
                { 1600, Detectors.bathroom }     // This move give us proper move to bathroom so other confusions can resolve faster because this decrease their probability
            }).Build();

            env.AdvanceToEnd((MotionDefaults.ConfusionResolutionTime / 2)); // We should resolve confusion 2x faster

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.hallwayLivingRoom).Should().BeFalse();
            env.LampState(Detectors.bathroom).Should().BeTrue();
            env.LampState(Detectors.hallwayToilet).Should().BeTrue();
        }

        

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                     2                                         |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |    1       |          |                      |
        // |        |                |    5       |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |     4      3                    |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Leave confused from room should not resolve other confusions")]
        public void Leave6()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 300, Detectors.bathroom },             // We have move in bathroom at start so future enter to bathroom is not sure and cannot cancel other confusions
                { 500, Detectors.hallwayLivingRoom },
                { 1000, Detectors.kitchen },
                { 1500, Detectors.hallwayToilet },
                { 1600, Detectors.bathroom }
            }).Build();

            env.AdvanceToEnd();

            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.LampState(Detectors.hallwayLivingRoom).Should().BeTrue();
            env.LampState(Detectors.bathroom).Should().BeTrue();
            env.LampState(Detectors.hallwayToilet).Should().BeTrue();

            env.AdvanceToIndex(4, confusionResolutionTime, true);

            env.LampState(Detectors.kitchen).Should().BeFalse("Kitchen should be OFF after confusion resolution time");
            env.LampState(Detectors.hallwayLivingRoom).Should().BeTrue();
            env.LampState(Detectors.bathroom).Should().BeTrue();
            env.LampState(Detectors.hallwayToilet).Should().BeTrue();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |             0    1                                            |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    2                            |
        // |        |                |            |____  ____|   3                  |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "When pass across the room light should turn off quickly")]
        public void Leave7()
        {
            var servieConfig = GetServiceBuilder().Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.livingRoom },
                { 2500, Detectors.hallwayLivingRoom },
                { 4500, Detectors.hallwayToilet },
                { 7500, Detectors.kitchen }
            }).Build();

            env.AdvanceToEnd();
        }



        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                  1,2,3,4,5                                    |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |    6     |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |                                 |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact(DisplayName = "Leave after move around room")]
        public void Leave5()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.hallwayLivingRoom },
                { 2500, Detectors.hallwayLivingRoom },
                { 4500, Detectors.hallwayLivingRoom },
                { 7500, Detectors.hallwayLivingRoom },
                { 10500, Detectors.hallwayLivingRoom },
                { 11500, Detectors.hallwayToilet },
            }).Build();

            env.AdvanceToEnd(confusionResolutionTime + TimeSpan.FromSeconds(15));
        }
    }
}