using HomeCenter.Actors.Tests.Builders;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.Configuration.DTO;
using Microsoft.Reactive.Testing;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Tests
{
    // Count number of people in house

    /* RavenDB query
         from @all_docs as e
          where e.lvl = "Information"
            and e.SourceContext = "HomeCenter.Services.MotionService.Room"
        select {
             Source : e.Room,
             Message: e.mt,
             Level: e.lvl,
             Time: e.RxTime,
             Event: e.EventId.Name,
             Vector: e.vector,
             Status: e.VectorStatus,
             BaseTimeOut: e.Statistics.BaseTimeOut,
             VisitType: e.Statistics.VisitType,
             Previous: e.Previous,
             Probability: e.Statistics.Probability,
             FirstEnterTime: e.Statistics.FirstEnterTime,
             NumberOfPersons: e.Statistics.NumberOfPersons,
             Delta: e.delta
         }
    */

    //                                      STAIRCASE [O]<1+>
    //  ________________________________________<_    __________________________
    // |        |                |                       |                      |
    // |        |                  [HL]<1+>  HALLWAY     |<                     |
    // |   B    |                |<            [H]<1+>                          |
    // |   A                     |___   ______           |       BADROOM        |
    // |   L    |                |            |          |         [S]<1+>      |
    // |   C    |                |            |          |                      |
    // |   O    |                |            |          |______________________|
    // |   N    |   LIVINGROOM  >|            |          |<                     |
    // |   Y    |      [L]<1+>   |  BATHROOM  | [HT]<1+> |                      |
    // |        |                |   [B]<1+> >|___v  ____|                      |
    // | [W]<1+>|                |            |          |       KITCHEN        |
    // |        |                |            |  TOILET  |         [K]<1+>      |
    // |        |                |            |  [T]<1>  |                      |
    // |_______v|________________|____________|_____v____|______________________|
    //
    // LEGEND: v/< - Motion Detector
    //         <x> - Max number of persons

    public abstract class LightAutomationServiceTestsBase : ReactiveTest
    {
        internal LightAutomationEnviromentBuilder GetEnviromentBuilder(ServiceDTO serviceConfig) =>
            new LightAutomationEnviromentBuilder(serviceConfig, RavenConfig.UseRavenDbLogs, RavenConfig.CleanLogsBeforeRun);

        /// <summary>
        /// Get predefined rooms configuration
        /// </summary>
        /// <returns></returns>
        internal LightAutomationServiceBuilder GetServiceBuilder()
        {
            var builder = new LightAutomationServiceBuilder();

            builder.WithRoom(new RoomBuilder(Rooms.Hallway).WithDetector(Detectors.hallwayToilet, new List<string> { Detectors.hallwayLivingRoom, Detectors.kitchen, Detectors.staircaseDetector, Detectors.toilet, Detectors.badroomDetector })
                                                           .WithDetector(Detectors.hallwayLivingRoom, new List<string> { Detectors.livingRoom, Detectors.bathroom, Detectors.hallwayToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Badroom).WithDetector(Detectors.badroomDetector, new List<string> { Detectors.hallwayToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Balcony).WithDetector(Detectors.balconyDetector, new List<string> { Detectors.livingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Bathroom).WithDetector(Detectors.bathroom, new List<string> { Detectors.hallwayLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Kitchen).WithDetector(Detectors.kitchen, new List<string> { Detectors.hallwayToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Livingroom).WithDetector(Detectors.livingRoom, new List<string> { Detectors.balconyDetector, Detectors.hallwayLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Staircase).WithDetector(Detectors.staircaseDetector, new List<string> { Detectors.hallwayToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Toilet).WithDetector(Detectors.toilet, new List<string> { Detectors.hallwayToilet }).WithProperty(MotionProperties.MaxPersonCapacity, 1));

            return builder;
        }
    }
}