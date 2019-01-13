﻿using Force.DeepCloner;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Messages.Events.Device;
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
    //TODO Add time between rooms - people walks 6km/1h => 6000m/3600s => 1m = 600ms
    //TODO Alarm when move in one enter room without move in entering neighbor
    [ProxyCodeGenerator]
    public class LightAutomationService : Service
    {
        private readonly IConcurrencyProvider _concurrencyProvider;

        //TODO is this necessary?
        private readonly IObservableTimer _observableTimer;

        private readonly MotionConfiguration _motionConfiguration;
        private readonly List<MotionVector> _confusedVectors = new List<MotionVector>();

        private ImmutableDictionary<string, Room> _rooms;
        private bool _IsInitialized;

        public LightAutomationService(IConcurrencyProvider concurrencyProvider,
                                      IMotionConfigurationProvider motionConfigurationProvider,
                                      IObservableTimer observableTimer
        )
        {
            var configurationProvider = motionConfigurationProvider ?? throw new ArgumentNullException(nameof(motionConfigurationProvider));
            _motionConfiguration = configurationProvider.GetConfiguration();
            _observableTimer = observableTimer;
            _concurrencyProvider = concurrencyProvider;
        }

        public Task Initialize()
        {
            _IsInitialized = true;
            return Task.CompletedTask;
        }

        public void RegisterRooms(IEnumerable<RoomInitializer> roomInitializers)
        {
            if (_IsInitialized) throw new Exception("Cannot register new descriptors after service has started");

            if (!roomInitializers.Any()) throw new Exception("No detectors found to automate");

            _rooms = roomInitializers.Select(roomInitializer => roomInitializer.ToRoom(_motionConfiguration, _concurrencyProvider, Logger, MessageBroker))
                                                             .ToImmutableDictionary(k => k.Uid, v => v);

            var missingRooms = _rooms.Select(m => m.Value)
                                     .SelectMany(n => n.Neighbors)
                                     .Distinct()
                                     .Except(_rooms.Keys)
                                     .ToList();
            if (missingRooms.Count > 0) throw new Exception($"Following neighbors have not registered rooms: {string.Join(", ", missingRooms)}");

            _rooms.Values.ForEach(room => room.BuildNeighborsCache(GetNeighbors(room.Uid)));
        }

        public void DisableAutomation(string roomId) => _rooms[roomId].DisableAutomation();

        public void DisableAutomation(string roomId, TimeSpan time) => _rooms[roomId].DisableAutomation(time);

        public void EnableAutomation(string roomId) => _rooms[roomId].EnableAutomation();

        public bool IsAutomationDisabled(string roomId) => _rooms[roomId].AutomationDisabled;

        public int GetCurrentNumberOfPeople(string roomId) => _rooms[roomId].NumberOfPersonsInArea;

        public AreaDescriptor GetAreaDescriptor(string roomId) => _rooms[roomId].AreaDescriptor.ShallowClone();

        public int NumberOfPersonsInHouse => _rooms.Sum(md => md.Value.NumberOfPersonsInArea);
        public int NumberOfConfusions => _confusedVectors.Count;
        public MotionConfiguration Configuration => _motionConfiguration.ShallowClone();

        public void Start()
        {
            _rooms.Values.ForEach(room => room.RegisterForLampChangeState());

            _disposables.Add(PeriodicCheck());
            _disposables.Add(CheckMotion());
        }

        public IObservable<MotionVector> AnalyzeMove()
        {
            var events = MessageBroker.Observe<MotionEvent>();

            return events.Timestamp(_concurrencyProvider.Scheduler)
                         .Select(move => new MotionWindow(move.Value.Message.MessageSource, move.Timestamp))
                         .Do(HandleMove)
                         .Window(events, _ => Observable.Timer(_motionConfiguration.MotionTimeWindow, _concurrencyProvider.Scheduler))
                         .SelectMany(x => x.Scan((vectors, currentPoint) => vectors.AccumulateVector(currentPoint.Start, IsProperVector))
                         .SelectMany(motion => motion.ToVectors()));
        }

        private void HandleMove(MotionWindow point)
        {
            _rooms?[point.Start.Uid]?.MarkMotion(point.Start.TimeStamp);
        }

        private IDisposable CheckMotion() => AnalyzeMove().ObserveOn(_concurrencyProvider.Task).Subscribe(HandleVector, HandleError);

        private void HandleVector(MotionVector motionVector)
        {
            var confusionPoints = _rooms[motionVector.End.Uid].GetConfusingPoints(motionVector);

            if (confusionPoints.Count == 0)
            {
                //TODO change this to async?
                MarkVector(motionVector).GetAwaiter().GetResult();
            }
            else if (_rooms[motionVector.Start.Uid].NumberOfPersonsInArea > 0)
            {
                Logger.LogInformation($"{motionVector} [Confused: {string.Join(" | ", confusionPoints)}]");
                _confusedVectors.Add(motionVector.Confuze(confusionPoints));
            }
            // If there is no more people in area this confusion is resolved immediately because it is physically impossible
            else
            {
                Logger.LogInformation($"{motionVector} [Deleted]");
            }
        }

        private void HandleError(Exception ex) => Logger.LogError(ex, "Exception in LightAutomationService");

        private IDisposable PeriodicCheck() => _observableTimer.GenerateTime(_motionConfiguration.PeriodicCheckTime)
                                                               .ObserveOn(_concurrencyProvider.Task)
                                                               .Subscribe(PeriodicCheck, HandleError);

        //TODO change this to async?
        private void PeriodicCheck(DateTimeOffset currentTime)
        {
            UpdateRooms().GetAwaiter().GetResult();
            ResolveConfusions(currentTime).GetAwaiter().GetResult();
        }

        private async Task ResolveConfusions(DateTimeOffset currentTime)
        {
            var toRemove = new HashSet<MotionVector>();

            foreach (var confusedVector in _confusedVectors)
            {
                // When timeout we have to delete confused vector
                if (currentTime.HappendBeforePrecedingTimeWindow(confusedVector.End.TimeStamp, _motionConfiguration.ConfusionResolutionTimeOut))
                {
                    toRemove.Add(confusedVector);
                    continue;
                }

                var startRoom = _rooms[confusedVector.Start.Uid];
                var endRoom = _rooms[confusedVector.End.Uid];

                var startNeighbors = startRoom.NeighborsCache.ToList().AddChained(startRoom).RemoveChained(endRoom);

                if (currentTime.HappendBeforePrecedingTimeWindow(confusedVector.Start.TimeStamp, _motionConfiguration.ConfusionResolutionTime))
                {
                    var noMoveInStartNeighbors = startNeighbors.All(n => n.LastMotion.Time.GetValueOrDefault() <= confusedVector.Start.TimeStamp);
                    if (noMoveInStartNeighbors)
                    {
                        await MarkVector(confusedVector.UnConfuze()).ConfigureAwait(false);
                        toRemove.Add(confusedVector);
                    }
                }
            }

            _confusedVectors.RemoveAll(toRemove.Contains);
        }

        private async Task UpdateRooms()
        {
            foreach (var room in _rooms.Values)
            {
                await room.Update().ConfigureAwait(false);
            }
        }

        private async Task MarkVector(MotionVector motionVector)
        {
            // If there was not confused vector from this point we don't start another
            if (_rooms[motionVector.End.Uid].LastVectorEnter?.EqualsWithStartTime(motionVector) ?? false) return;

            Logger.LogInformation(motionVector.ToString());

            await _rooms[motionVector.Start.Uid].MarkLeave(motionVector).ConfigureAwait(false);
            _rooms[motionVector.End.Uid].MarkEnter(motionVector);
        }

        private bool IsProperVector(MotionPoint start, MotionPoint potencialEnd) => AreNeighbors(start, potencialEnd) && start.IsMovePhisicallyPosible(potencialEnd, _motionConfiguration.MotionMinDiff);

        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _rooms?[p1.Uid]?.Neighbors?.Contains(p2.Uid) ?? false;

        private IEnumerable<Room> GetNeighbors(string roomId) => _rooms.Where(x => _rooms[roomId].Neighbors.Contains(x.Key)).Select(y => y.Value);
    }
}