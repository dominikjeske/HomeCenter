using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    //TODO Add time between rooms - people walks 6km/1h => 6000m/3600s => 1m = 600ms
    //TODO Alarm when move in one enter room without move in entering neighbor
    [ProxyCodeGenerator]
    public abstract class LightAutomationService : Service
    {
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly List<MotionVector> _confusedVectors = new List<MotionVector>();
        private readonly MotionConfiguration _motionConfiguration = new MotionConfiguration();
        private ImmutableDictionary<string, Room> _rooms;
        private readonly IObservableTimer _observableTimer;

        protected LightAutomationService(IConcurrencyProvider concurrencyProvider, IObservableTimer observableTimer)
        {
            _concurrencyProvider = concurrencyProvider;
            _observableTimer = observableTimer;
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            ReadConfigurationFromProperties();
            var areas = ReadAreasFromAttachedProperties();
            ReadRoomsFromAttachedProperties(areas);

            StartWatchForEvents();
        }

        private Dictionary<string, AreaDescriptor> ReadAreasFromAttachedProperties()
        {
            var areas = new Dictionary<string, AreaDescriptor>();
            foreach (var area in AreasAttachedProperties)
            {
                areas.Add(area.AttachedActor, new AreaDescriptor
                {
                    WorkingTime = area.AsString(MotionProperties.WorkingTime, WorkingTime.AllDay),
                    MaxPersonCapacity = area.AsInt(MotionProperties.MaxPersonCapacity, 10),
                    AreaType = area.AsString(MotionProperties.AreaType, AreaType.Room),
                    MotionDetectorAlarmTime = area.AsTime(MotionProperties.MotionDetectorAlarmTime, TimeSpan.FromMilliseconds(2500)),
                    LightIntensityAtNight = area.ContainsProperty(MotionProperties.LightIntensityAtNight) ? (double?)area.AsDouble(MotionProperties.LightIntensityAtNight) : null,
                    TurnOffTimeout = area.AsTime(MotionProperties.TurnOffTimeout, TimeSpan.FromMilliseconds(10000)),
                    TurnOffAutomationDisabled = area.AsBool(MotionProperties.TurnOffAutomationDisabled, false)
                });
            }
            return areas;
        }

        private void StartWatchForEvents()
        {
            _rooms.Values.ForEach(room => room.RegisterForLampChangeState());

            _disposables.Add(PeriodicCheck());
            _disposables.Add(CheckMotion());
        }

        private void ReadConfigurationFromProperties()
        {
            _motionConfiguration.MotionTimeWindow = AsTime(MotionProperties.MotionTimeWindow, TimeSpan.FromMilliseconds(3000));
            _motionConfiguration.ConfusionResolutionTime = AsTime(MotionProperties.ConfusionResolutionTime, TimeSpan.FromMilliseconds(5000));
            _motionConfiguration.ConfusionResolutionTimeOut = AsTime(MotionProperties.ConfusionResolutionTimeOut, TimeSpan.FromMilliseconds(10000));
            _motionConfiguration.MotionMinDiff = AsTime(MotionProperties.MotionMinDiff, TimeSpan.FromMilliseconds(500));
            _motionConfiguration.PeriodicCheckTime = AsTime(MotionProperties.PeriodicCheckTime, TimeSpan.FromMilliseconds(1000));
            _motionConfiguration.ManualCodeWindow = AsTime(MotionProperties.MotionTimeWindow, TimeSpan.FromMilliseconds(3000));
            _motionConfiguration.TurnOffTimeoutIncrementPercentage = AsDouble(MotionProperties.TurnOffTimeoutIncrementPercentage, 50);
            _motionConfiguration.TurnOffPresenceFactor = AsDouble(MotionProperties.TurnOffPresenceFactor, 0.05f);
        }

        private void ReadRoomsFromAttachedProperties(Dictionary<string, AreaDescriptor> areas)
        {
            List<Room> rooms = new List<Room>();
            foreach (var motionDetector in ComponentsAttachedProperties)
            {
                rooms.Add(new Room(motionDetector.AttachedActor, motionDetector.AsList(MotionProperties.Neighbors), motionDetector.AsString(MotionProperties.Lamp),
                                    _concurrencyProvider, Logger, MessageBroker, areas[motionDetector.AttachedArea], _motionConfiguration));
            }

            _rooms = rooms.ToImmutableDictionary(k => k.Uid, v => v);

            var missingRooms = _rooms.Select(m => m.Value)
                                     .SelectMany(n => n._neighbors)
                                     .Distinct()
                                     .Except(_rooms.Keys)
                                     .ToList();
            if (missingRooms.Count > 0) throw new ConfigurationException($"Following neighbors have not registered rooms: {string.Join(", ", missingRooms)}");

            _rooms.Values.ForEach(room => room.BuildNeighborsCache(GetNeighbors(room.Uid)));
        }

        protected bool Handle(IsAliveQuery isAliveQuery)
        {
            return true;
        }

        protected void Handle(DisableAutomationCommand command)
        {
            var roomId = command.AsString(MotionProperties.RoomId);
            if (command.ContainsProperty(MessageProperties.TimeOut))
            {
                _rooms[roomId].DisableAutomation(command.AsTime(MessageProperties.TimeOut));
            }
            else
            {
                _rooms[roomId].DisableAutomation();
            }
        }

        protected void Handle(EnableAutomationCommand command)
        {
            var roomId = command.AsString(MotionProperties.RoomId);
            _rooms[roomId].EnableAutomation();
        }

        protected bool Handle(AutomationStateQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _rooms[roomId].AutomationDisabled;
        }

        protected int Handle(NumberOfPeopleQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _rooms[roomId].NumberOfPersonsInArea;
        }

        protected AreaDescriptor Handle(AreaDescriptorQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _rooms[roomId]._areaDescriptor.Clone();
        }

        protected MotionStatus Handle(MotionServiceStatusQuery query)
        {
            return new MotionStatus
            {
                NumberOfPersonsInHouse = _rooms.Sum(md => md.Value.NumberOfPersonsInArea),
                NumberOfConfusions = _confusedVectors.Count
            };
        }

        private IObservable<MotionVector> AnalyzeMove()
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
            _rooms?[point.Start.Uid]?.MarkMotion(point.Start.TimeStamp).GetAwaiter().GetResult();
        }

        private IDisposable CheckMotion() => AnalyzeMove().ObserveOn(_concurrencyProvider.Task).Subscribe(HandleVector, HandleError);

        private void HandleVector(MotionVector motionVector)
        {
            var confusionPoints = _rooms[motionVector.End.Uid].GetConfusingPoints(motionVector);

            if (confusionPoints.Count == 0)
            {
                //TODO change this to Async?
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

        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _rooms?[p1.Uid]?._neighbors?.Contains(p2.Uid) ?? false;

        private IEnumerable<Room> GetNeighbors(string roomId) => _rooms.Where(x => _rooms[roomId]._neighbors.Contains(x.Key)).Select(y => y.Value);
    }
}