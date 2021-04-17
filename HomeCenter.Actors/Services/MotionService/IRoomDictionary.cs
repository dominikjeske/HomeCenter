using System;
using System.Collections.Generic;
using HomeCenter.Services.MotionService.Model;

namespace HomeCenter.Services.MotionService
{
    internal interface IRoomDictionary
    {
        Room this[string uid] { get; }

        void EvaluateRooms(DateTimeOffset motionTime);

        MotionVector? GetLastLeaveVector(MotionVector motionVector);

        void HandleVectors(IList<MotionVector> motionVectors);

        bool IsProperVector(MotionPoint start, MotionPoint potencialEnd);

        void MarkLeave(MotionVector motionVector);

        void MarkMotion(MotionWindow point);

        bool MoveInNeighborhood(string roomid, string roomToExclude, DateTimeOffset referenceTime);

        int NumberOfPersons();
    }
}