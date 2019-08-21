using HomeCenter.Services.MotionService.Model;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class RoomService
    {
        private readonly IReadOnlyDictionary<string, Room> _rooms;
        private readonly MotionConfiguration _motionConfiguration;
        private readonly ILogger _logger;

        public Room this[string uid]
        {
            get { return _rooms[uid]; }
        }

        private Room this[MotionWindow window]
        {
            get { return _rooms[window.Start.Uid]; }
        }

        private Room this[MotionVector vector]
        {
            get { return _rooms[vector.EndPoint]; }
        }

        public RoomService(IEnumerable<Room> rooms, MotionConfiguration motionConfiguration, ILogger logger)
        {
            _rooms = rooms.ToDictionary(k => k.Uid, v => v).AsReadOnly();
            _motionConfiguration = motionConfiguration;
            _logger = logger;
        }

        public void RegisterForLampChangeState()
        {
            _rooms.Values.ForEach(room => room.RegisterForLampChangeState());
        }

        public int NumberOfPersons() => _rooms.Sum(md => md.Value.NumberOfPersons);

        /// <summary>
        /// Check if two point in time can physically be a proper vector
        /// </summary>
        /// <param name="start"></param>
        /// <param name="potencialEnd"></param>
        /// <returns></returns>
        public bool IsProperVector(MotionPoint start, MotionPoint potencialEnd)
        {
            return AreNeighbors(start, potencialEnd) && potencialEnd.IsMovePhisicallyPosible(start, _motionConfiguration.MotionMinDiff);
        }

        public async Task HandleVectors(IList<MotionVector> motionVectors)
        {
            if (motionVectors.Count == 0) return;

            // When we have one vector we know that there is no concurrent vectors to same room
            else if (motionVectors.Count == 1)
            {
                var vector = motionVectors.Single();
                var targetRoom = this[vector];
                if (targetRoom.IsTurnOnVector(vector))
                {
                    await MarkVector(vector);
                }
                else
                {
                    targetRoom.MarkConfusion(vector);
                }
            }
            // When we have at least two vectors we know that this vector is confused
            else
            {
                motionVectors.ForEach(vector => this[vector].MarkConfusion(vector));
            }
        }

        public Task MarkMotion(MotionWindow point)
        {
            return this[point].MarkMotion(point.Start.TimeStamp);
        }

        /// <summary>
        /// Evaluates each room state
        /// </summary>
        public async Task UpdateRooms(DateTimeOffset motionTime)
        {
            await _rooms.Values.Select(async r => await r.EvaluateConfusions(motionTime)).WhenAll();

            await _rooms.Values.Select(async r => await r.PeriodicUpdate(motionTime)).WhenAll();
        }

        /// <summary>
        /// Marks enter to target room and leave from source room
        /// </summary>
        /// <param name="motionVector"></param>
        /// <returns></returns>
        private async Task MarkVector(MotionVector motionVector)
        {
            var targetRoom = _rooms[motionVector.EndPoint];
            var sourceRoom = _rooms[motionVector.StartPoint];

            _logger.LogInformation(motionVector.ToString());

            await sourceRoom.MarkLeave(motionVector);
            targetRoom.MarkEnter(motionVector);
        }

        /// <summary>
        /// Check if two points are neighbors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _rooms[p1.Uid].IsNeighbor(p2.Uid);
    }
}