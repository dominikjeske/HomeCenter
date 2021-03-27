using ConcurrentCollections;
using HomeCenter.Extensions;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace HomeCenter.Services.MotionService
{
    internal class ConfusedVectors
    {
        private readonly ConcurrentHashSet<MotionVector> _confusingVectors = new();
        private readonly ILogger _logger;
        private readonly string _uid;
        private readonly TimeSpan _confusionResolutionTime;
        private readonly TimeSpan _confusionResolutionTimeOut;
        private readonly Lazy<RoomDictionary> _roomDictionary;
        private readonly Subject<MotionVector> _resolvedObservable = new();
        public IObservable<MotionVector> Resolved => _resolvedObservable;

        public ConfusedVectors(ILogger logger, string uid, TimeSpan confusionResolutionTime, TimeSpan confusionResolutionTimeOut,
            Lazy<RoomDictionary> roomDictionary)
        {
            _logger = logger;
            _uid = uid;
            _confusionResolutionTime = confusionResolutionTime;
            _confusionResolutionTimeOut = confusionResolutionTimeOut;
            _roomDictionary = roomDictionary;
        }

        /// <summary>
        /// Try to resolve confusion in previously marked vectors
        /// </summary>
        public void EvaluateConfusions(DateTimeOffset currentTime)
        {
            GetConfusedVectorsAfterTimeout(currentTime).Where(vector => NoMoveInStartNeighbors(vector))
                                                                 .ForEach(v => ResolveConfusion(v));

            GetConfusedVecotrsCanceledByOthers().ForEach(v => TryResolveAfterCancel(v));
        }

        /// <summary>
        /// Check if there were any moves in neighbors of starting point of <paramref name="vector"/>. This indicates that <paramref name="vector"/> is not confused.
        /// </summary>
        private bool NoMoveInStartNeighbors(MotionVector vector)
        {
            var moveInStartNeighbors = _roomDictionary.Value.MoveInNeighborhood(vector.StartPoint, _uid, vector.StartTime);
            return !moveInStartNeighbors;
        }

        /// <summary>
        /// After we remove canceled vector we check if there is other vector in same time that was in confusion. When there is only one we can resolve it because there is no confusion anymore
        /// </summary>
        private void TryResolveAfterCancel(MotionVector motionVector)
        {
            _logger.LogDebug(MoveEventId.VectorCancel, "{vector} [Cancel]", motionVector);

            RemoveConfusedVector(motionVector);

            var confused = _confusingVectors.Where(x => x.End == motionVector.End);
            if (confused.Count() == 1)
            {
                ResolveConfusion(confused.Single());
            }
        }

        /// <summary>
        /// When there is approved leave vector from one of the source rooms we have confused vectors in same time we can assume that our vector is not real and we can remove it in shorter time
        /// </summary>
        private IEnumerable<MotionVector> GetConfusedVecotrsCanceledByOthers()
        {
            //!!!!!!TODO
            //&& currentTime.Between(v.EndTime).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime / 2)
            return _confusingVectors.Where(v => _roomDictionary.Value[v.StartPoint].RoomStatistic.LastLeaveVector?.Start == v.Start);
        }

        /// <summary>
        /// Mark some motion that can be enter vector but we are not sure
        /// </summary>
        public void MarkConfusion(IList<MotionVector> vectors)
        {
            foreach (var vector in vectors)
            {
                _logger.LogInformation(MoveEventId.ConfusedVector, "{vector} with {VectorStatus}", vector, "Confused");

                _confusingVectors.Add(vector);
            }
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
        private void ResolveConfusion(MotionVector vector)
        {
            RemoveConfusedVector(vector);

            _resolvedObservable.OnNext(vector);
        }
    }
}