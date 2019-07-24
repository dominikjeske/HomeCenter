using HomeCenter.Services.MotionService.Model;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class RoomService
    {
        private readonly ImmutableDictionary<string, Room> _rooms;
        private readonly ImmutableDictionary<Room, IEnumerable<Room>> _neighbors;
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

        public RoomService(IEnumerable<Room> rooms, MotionConfiguration motionConfiguration, ILogger logger)
        {
            _rooms = rooms.ToImmutableDictionary(k => k.Uid, v => v);

            var dic = new Dictionary<Room, IEnumerable<Room>>();
            foreach (var room in rooms)
            {
                dic.Add(room, room.Neighbors().Select(n => _rooms[n]));
            }

            _neighbors = dic.ToImmutableDictionary();
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
                var targetRoom = GetTargetRoom(vector);
                // When whis vector was responsible for turning on the light we are sure we can mark enter
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
                motionVectors.ForEach(vector => _rooms[vector.EndPoint].MarkConfusion(vector));
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
            await EvaluateConfusions(motionTime);

            await _rooms.Values.Select(async r => await r.PeriodicUpdate(motionTime)).WhenAll();
        }

        /// <summary>
        /// Marks enter to target room and leave from source room
        /// </summary>
        /// <param name="motionVector"></param>
        /// <returns></returns>
        private async Task MarkVector(MotionVector motionVector)
        {
            var targetRoom = GetTargetRoom(motionVector);
            var sourceRoom = GetSourceRoom(motionVector);

            _logger.LogInformation(motionVector.ToString());

            await sourceRoom.MarkLeave(motionVector);
            targetRoom.MarkEnter(motionVector);
        }

        /// <summary>
        /// Try to resolve confusion in previously marked vectors
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        private async Task EvaluateConfusions(DateTimeOffset currentTime)
        {
            foreach (var room in _rooms.Values)
            {
                var confusedVectors = room.GetConfusionsToResolve(currentTime).OrderByDescending(v => v.EndTime);

                var uncofused = new List<MotionVector>();
                foreach(var vector in confusedVectors)
                {
                    if(NoMoveInStartNeighbors(vector))
                    {
                        room.ResolveConfusion(vector);

                        await GetSourceRoom(vector).MarkLeave(vector);
                        // confused vectors from same time spot should change person probability (but not for 100%)
                    }
                }

                
            }
        }

        /// <summary>
        /// Check if two points are neighbors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _rooms[p1.Uid].IsNeighbor(p2.Uid);
               
        /// <summary>
        /// Get room pointed by end of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private Room GetTargetRoom(MotionVector motionVector) => _rooms[motionVector.EndPoint];

        /// <summary>
        /// Get room pointed by beginning of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private Room GetSourceRoom(MotionVector motionVector) => _rooms[motionVector.StartPoint];

        /// <summary>
        /// Check if there were any moves in neighbors of starting point of <paramref name="vector"/>. This indicates that <paramref name="vector"/> is not confused.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private bool NoMoveInStartNeighbors(MotionVector vector)
        {
            var sourceRoom = GetSourceRoom(vector);
            var endRoom = GetTargetRoom(vector);

            var startNeighbors = _neighbors[sourceRoom].ToList().AddChained(sourceRoom).RemoveChained(endRoom);

            return startNeighbors.All(n => n.LastMotion.Time <= vector.StartTime);
        }
    }
}