using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ConcurrentCollections;
using HomeCenter.Extensions;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;

namespace HomeCenter.Services.MotionService
{
    internal class ConfusedVectors
    {
        internal class VectorResolution
        {
            public static readonly VectorResolution Empty = new(MotionVector.Empty, string.Empty);

            public VectorResolution(MotionVector vector, string Reason)
            {
                Vector = vector;
                this.Reason = Reason;
            }

            public MotionVector Vector { get; }

            public string Reason { get; }
        }

        private readonly ConcurrentHashSet<MotionVector> _confusingVectors = new();
        private readonly ILogger _logger;
        private readonly string _uid;
        private readonly TimeSpan _confusionResolutionTime;
        private readonly TimeSpan _confusionResolutionTimeOut;
        private readonly Lazy<RoomDictionary> _roomDictionary;

        public ConfusedVectors(ILogger logger, string uid, TimeSpan confusionResolutionTime, TimeSpan confusionResolutionTimeOut,
            Lazy<RoomDictionary> roomDictionary)
        {
            _logger = logger;
            _uid = uid;
            _confusionResolutionTime = confusionResolutionTime;
            _confusionResolutionTimeOut = confusionResolutionTimeOut;
            _roomDictionary = roomDictionary;
        }

        public bool HasEntryConfusions => _confusingVectors.Count > 0;

        /// <summary>
        /// Mark some motion that can be enter vector but we are not sure.
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
        /// Try to resolve confusion in previously marked vectors.
        /// </summary>
        public IEnumerable<MotionVector> EvaluateConfusions(DateTimeOffset currentTime)
        {
            List<VectorResolution> resolved = new();
            List<VectorResolution> canceled = new();

            // 1. Resolve vectors that have time out
            resolved.AddRange(ResolveAfterTimeout(currentTime));
            // 2. Cancel confused by real vector from same starting point
            canceled.AddRange(CancelAfterStartPoint());
            // 3. Cancel vectors by previously resolved
            canceled.AddRange(ResolveByEndPoint(resolved, canceled));
            // 4. Resolve vectors by previously canceled
            resolved.AddRange(ResolveByEndPoint(canceled, resolved));

            foreach (var vector in resolved)
            {
                _logger.LogInformation(MoveEventId.Resolution, "{vector} resolved by {VectorStatus}", vector.Vector, vector.Reason);
                _confusingVectors.TryRemove(vector.Vector);
            }

            foreach (var vector in canceled)
            {
                _logger.LogInformation(MoveEventId.VectorCancel, "{vector} canceled by {VectorStatus}", vector.Vector, vector.Reason);
                _confusingVectors.TryRemove(vector.Vector);
            }

            return resolved.Select(v => v.Vector);
        }

        private List<VectorResolution> ResolveByEndPoint(IEnumerable<VectorResolution> vectors, IEnumerable<VectorResolution> skip)
        {
            List<VectorResolution> autoResolved = new();

            foreach (var vector in vectors)
            {
                if (FindByEndPoint(vector.Vector, skip, out var newResolved))
                {
                    autoResolved.Add(newResolved);
                }
            }

            return autoResolved;
        }

        private IEnumerable<VectorResolution> ResolveAfterTimeout(DateTimeOffset currentTime)
        {
            return GetConfusedVectorsAfterTimeout(currentTime)
                  .Where(vector => NoMoveInStartNeighbors(vector))
                  .Select(v => new VectorResolution(v, "Timeout"));
        }

        /// <summary>
        /// Get list of all confused vectors that should be resolved.
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

        /// <summary>
        /// Check if there were any moves in neighbors of starting point of <paramref name="vector"/>. This indicates that <paramref name="vector"/> is not confused.
        /// </summary>
        private bool NoMoveInStartNeighbors(MotionVector vector)
        {
            var moveInStartNeighbors = _roomDictionary.Value.MoveInNeighborhood(vector.StartPoint, _uid, vector.StartTime);
            return !moveInStartNeighbors;
        }

        /// <summary>
        /// When there is approved leave vector from one of the source rooms we have confused vectors in same time
        /// we can assume that our vector is not real and we can remove it in shorter time.
        /// </summary>
        private IEnumerable<VectorResolution> CancelAfterStartPoint()
        {
            return _confusingVectors.Where(v => LeaveVectorsWithSameStart(v))
                                    .Select(v => new VectorResolution(v, $"Cancel by source leave '{v}'"));
        }

        private bool LeaveVectorsWithSameStart(MotionVector v)
        {


            return _roomDictionary.Value.GetLastLeaveVector(v)?.Start == v.Start;
        }

        private bool FindByEndPoint(MotionVector resolvedVecotr, IEnumerable<VectorResolution> skip, out VectorResolution newResolved)
        {
            var confused = _confusingVectors.Where(vector => vector.End == resolvedVecotr.End && vector != resolvedVecotr && !skip.Any(s => s.Vector == vector));
            if (confused.Count() == 1)
            {
                newResolved = new VectorResolution(confused.Single(), $"Auto resolved by '{resolvedVecotr}'");
                return true;
            }

            newResolved = VectorResolution.Empty;
            return false;
        }
    }
}