using HomeCenter.Services.MotionService.Model;
using HomeCenter.Utils.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class RoomService
    {
        private ImmutableDictionary<string, Room> _rooms;
        private Dictionary<Room, IEnumerable<Room>> _neighbors = new Dictionary<Room, IEnumerable<Room>>();

        public RoomService(IEnumerable<Room> rooms)
        {
            _rooms = rooms.ToImmutableDictionary(k => k.Uid, v => v);

            foreach (var room in rooms)
            {
                _neighbors.Add(room, room.Neighbors().Select(n => _rooms[n]));
            }
        }

        public void RegisterForLampChangeState()
        {
            _rooms.Values.ForEach(room => room.RegisterForLampChangeState());
        }

        /// <summary>
        /// Evaluates each room state
        /// </summary>
        public async Task UpdateRooms()
        {
            foreach (var room in _rooms.Values)
            {
                await room.PeriodicUpdate();
            }
        }

        /// <summary>
        /// Check if two points are neighbors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _rooms[p1.Uid].IsNeighbor(p2.Uid);

        /// <summary>
        /// Get all neighbors of given room
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public IEnumerable<Room> GetNeighbors(string roomId) => _rooms.Where(x => _rooms[roomId].IsNeighbor(x.Key)).Select(y => y.Value);

        /// <summary>
        /// Get room pointed by end of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        public Room GetTargetRoom(MotionVector motionVector) => _rooms[motionVector.EndPoint];

        /// <summary>
        /// Get room pointed by beginning of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        public Room GetSourceRoom(MotionVector motionVector) => _rooms[motionVector.StartPoint];

        public Room this[string uid]
        {
            get { return _rooms[uid]; }
        }

        public Room this[MotionWindow window]
        {
            get { return _rooms[window.Start.Uid]; }
        }

        public int NumberOfPersons() => _rooms.Sum(md => md.Value.NumberOfPersonsInArea);

        public bool NoMoveInStartNeighbors(MotionVector confusedVector)
        {
            var sourceRoom = GetSourceRoom(confusedVector);
            var endRoom = this[confusedVector.EndPoint];

            var startNeighbors = _neighbors[sourceRoom].ToList().AddChained(sourceRoom).RemoveChained(endRoom);

            return startNeighbors.All(n => n.LastMotion.Time.GetValueOrDefault() <= confusedVector.StartTime);
        }

        /// <summary>
        /// Get confusion point from all neighbors
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public IList<MotionPoint> GetConfusingPoints(MotionVector vector)
        {
            var targetRoom = GetTargetRoom(vector);
            return _neighbors[targetRoom].ToList()
                                         .AddChained(targetRoom)
                                         .Where(room => room.Uid != vector.StartPoint)
                                         .Select(room => room.GetConfusion(vector.EndTime))
                                         .Where(y => y != MotionPoint.Empty)
                                         .ToList();
        }
    }
}