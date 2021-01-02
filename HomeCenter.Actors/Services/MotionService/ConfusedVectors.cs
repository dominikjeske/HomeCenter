using ConcurrentCollections;
using HomeCenter.Abstractions.Extensions;
using HomeCenter.Extensions;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class ConfusedVectors
    {
        private readonly ConcurrentHashSet<MotionVector> _confusingVectors = new ConcurrentHashSet<MotionVector>();
        private readonly ILogger _logger;
        private readonly string _uid;
        private readonly TimeSpan _confusionResolutionTime;
        private readonly TimeSpan _confusionResolutionTimeOut;
        private readonly Func<MotionVector, Room> _sourceRoomResolver;
        private readonly Func<MotionVector, Task> _markRoom;

        public ConfusedVectors(ILogger logger, string uid, TimeSpan confusionResolutionTime, TimeSpan confusionResolutionTimeOut,
            Func<MotionVector, Room> sourceRoomResolver, Func<MotionVector, Task> markRoom)
        {
            _logger = logger;
            _uid = uid;
            _confusionResolutionTime = confusionResolutionTime;
            _confusionResolutionTimeOut = confusionResolutionTimeOut;
            _sourceRoomResolver = sourceRoomResolver;
            _markRoom = markRoom;
        }

        /// <summary>
        /// Try to resolve confusion in previously marked vectors
        /// </summary>
        public async Task EvaluateConfusions(DateTimeOffset currentTime)
        {
            await GetConfusedVectorsAfterTimeout(currentTime).Where(vector => NoMoveInStartNeighbors(vector))
                                                             .Select(v => ResolveConfusion(v))
                                                             .WhenAll();

            await GetConfusedVecotrsCanceledByOthers(currentTime).Select(v => TryResolveAfterCancel(v))
                                                                 .WhenAll();
        }

        /// <summary>
        /// Check if there were any moves in neighbors of starting point of <paramref name="vector"/>. This indicates that <paramref name="vector"/> is not confused.
        /// </summary>
        private bool NoMoveInStartNeighbors(MotionVector vector)
        {
            var sourceRoom = _sourceRoomResolver(vector);
            var moveInStartNeighbors = sourceRoom.MoveInNeighborhood(_uid, vector.StartTime);
            return !moveInStartNeighbors;
        }

        /// <summary>
        /// After we remove canceled vector we check if there is other vector in same time that was in confusion. When there is only one we can resolve it because there is no confusion anymore
        /// </summary>
        private Task TryResolveAfterCancel(MotionVector motionVector)
        {
            _logger.LogDeviceEvent(_uid, MoveEventId.VectorCancel, "{motionVector} [Cancel]", motionVector);

            RemoveConfusedVector(motionVector);

            var confused = _confusingVectors.Where(x => x.End == motionVector.End);
            if (confused.Count() == 1)
            {
                return ResolveConfusion(confused.Single());
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// When there is approved leave vector from one of the source rooms we have confused vectors in same time we can assume that our vector is not real and we can remove it in shorter time
        /// </summary>
        private IEnumerable<MotionVector> GetConfusedVecotrsCanceledByOthers(DateTimeOffset currentTime)
        {
            //!!!!!!TODO
            //&& currentTime.Between(v.EndTime).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime / 2)
            return _confusingVectors.Where(v => _sourceRoomResolver(v)._roomStatistic.LastLeaveVector?.Start == v.Start);
        }

        /// <summary>
        /// Mark some motion that can be enter vector but we are not sure
        /// </summary>
        public void MarkConfusion(MotionVector vector)
        {
            _logger.LogDeviceEvent(_uid, MoveEventId.ConfusedVector, "Confused vector {vector}", vector);

            _confusingVectors.Add(vector);
        }

        /// <summary>
        /// Get list of all confused vectors that should be resolved
        /// </summary>
        private IEnumerable<MotionVector> GetConfusedVectorsAfterTimeout(DateTimeOffset currentTime)
        {
            var confusedReadyToResolve = _confusingVectors.Where(t => currentTime.Between(t.EndTime)
                                                          .LastedLongerThen(_confusionResolutionTime));

            // When all vectors are older then timeout we cannot resolve confusions
            if (!confusedReadyToResolve.Any(vector => currentTime.Between(vector.EndTime)
                                                                 .LastedLessThen(_confusionResolutionTimeOut)))
            {
                return Enumerable.Empty<MotionVector>();
            }
            return confusedReadyToResolve;
        }

        private void RemoveConfusedVector(MotionVector vector)
        {
            _confusingVectors.TryRemove(vector);
        }

        /// <summary>
        /// Executed when after some time we can resolve confused vectors
        /// </summary>
        private async Task ResolveConfusion(MotionVector vector)
        {
            RemoveConfusedVector(vector);

            await _markRoom(vector);
        }
    }
}