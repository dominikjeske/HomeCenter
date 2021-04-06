
    /* RavenDB query
         from @all_docs as e
          where e.lvl = "Information"
            and e.SourceContext = "HomeCenter.Services.MotionService.Room"
       order by e.RxTime
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
