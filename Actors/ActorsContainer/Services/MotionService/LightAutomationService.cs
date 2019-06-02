using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
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

        protected LightAutomationService(IConcurrencyProvider concurrencyProvider)
        {
            _concurrencyProvider = concurrencyProvider;
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context);

            ReadConfigurationFromProperties();
            var areas = ReadAreasFromAttachedProperties();
            ReadRoomsFromAttachedProperties(areas);

            StartWatchForEvents();
        }

        private void ReadConfigurationFromProperties()
        {
            _motionConfiguration.MotionTimeWindow = AsTime(MotionProperties.MotionTimeWindow, TimeSpan.FromMilliseconds(3000));
            _motionConfiguration.ConfusionResolutionTime = AsTime(MotionProperties.ConfusionResolutionTime, TimeSpan.FromMilliseconds(5000));
            _motionConfiguration.ConfusionResolutionTimeOut = AsTime(MotionProperties.ConfusionResolutionTimeOut, TimeSpan.FromMilliseconds(10000));
            _motionConfiguration.MotionMinDiff = AsTime(MotionProperties.MotionMinDiff, TimeSpan.FromMilliseconds(500));
            _motionConfiguration.PeriodicCheckTime = AsTime(MotionProperties.PeriodicCheckTime, TimeSpan.FromMilliseconds(1000));
            _motionConfiguration.ManualCodeWindow = AsTime(MotionProperties.ManualCodeWindow, TimeSpan.FromMilliseconds(3000));
            _motionConfiguration.TurnOffTimeoutExtenderFactor = AsDouble(MotionProperties.TurnOffTimeoutIncrementPercentage, 50);
            _motionConfiguration.TurnOffTimeoutIncrementFactor = AsDouble(MotionProperties.TurnOffPresenceFactor, 0.05f);
        }

        private Dictionary<string, AreaDescriptor> ReadAreasFromAttachedProperties()
        {
            var areas = new Dictionary<string, AreaDescriptor>();
            foreach (var area in AreasAttachedProperties)
            {
                areas.Add(area.AttachedActor, new AreaDescriptor()
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

        private void ReadRoomsFromAttachedProperties(Dictionary<string, AreaDescriptor> areas)
        {
            var rooms = new List<Room>();
            foreach (var motionDetector in ComponentsAttachedProperties)
            {
                rooms.Add(new Room(motionDetector.AttachedActor, motionDetector.AsList(MotionProperties.Neighbors), motionDetector.AsString(MotionProperties.Lamp),
                                    _concurrencyProvider, Logger, MessageBroker, areas.Get(motionDetector.AttachedArea, AreaDescriptor.Default), _motionConfiguration));
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

        private void StartWatchForEvents()
        {
            _rooms.Values.ForEach(room => room.RegisterForLampChangeState());

            PeriodicCheck();
            AnalyzeMove();
        }

        /// <summary>
        /// Query motion events from system and analyses move
        /// </summary>
        private void AnalyzeMove()
        {
            var events = MessageBroker.Observe<MotionEvent>();

            var motionWindows = events.Timestamp(_concurrencyProvider.Scheduler)
                                      .Select(move => new MotionWindow(move.Value.Message.MessageSource, move.Timestamp));

            motionWindows.Subscribe(HandleMove, HandleError, Token, _concurrencyProvider.Task);

            motionWindows.Window(events, _ => Observable.Timer(_motionConfiguration.MotionTimeWindow, _concurrencyProvider.Scheduler))
                         .SelectMany(x => x.Scan((vectors, currentPoint) => vectors.AccumulateVector(currentPoint.Start, IsProperVector))
                         .SelectMany(motion => motion.ToVectors()))
                         .Subscribe(HandleVector, HandleError, Token, _concurrencyProvider.Task);
        }

        /// <summary>
        /// Handle move inside a room
        /// </summary>
        /// <param name="point"></param>
        private Task HandleMove(MotionWindow point) => GetRoom(point).MarkMotion(point.Start.TimeStamp);

        /// <summary>
        /// Handle move between rooms represented by <seealso cref="MotionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private async Task HandleVector(MotionVector motionVector)
        {
            var confusionPoints = GetTargetRoom(motionVector).GetConfusingPoints(motionVector);

            if (confusionPoints.Count == 0)
            {
                await MarkVector(motionVector);
            }
            else if (GetSourceRoom(motionVector).NumberOfPersonsInArea > 0)
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

        /// <summary>
        /// Exception logger for all Rx queries
        /// </summary>
        /// <param name="ex"></param>
        private void HandleError(Exception ex) => Logger.LogError(ex, "Exception in LightAutomationService");

        /// <summary>
        /// Checks periodically all rooms for current state - parallel to move
        /// </summary>
        private void PeriodicCheck()
        {
            Observable.Interval(_motionConfiguration.PeriodicCheckTime, _concurrencyProvider.Scheduler)
                      .Timestamp(_concurrencyProvider.Scheduler)
                      .Select(time => time.Timestamp)
                      .ObserveOn(_concurrencyProvider.Task)
                      .Subscribe(PeriodicCheck, HandleError, Token, _concurrencyProvider.Task);
        }

        /// <summary>
        /// Check rooms state on given <paramref name="currentTime"/>
        /// </summary>
        /// <param name="currentTime">Time when we are checking</param>
        /// <returns></returns>
        private async Task PeriodicCheck(DateTimeOffset currentTime)
        {
            await UpdateRooms();
            await ResolveConfusions(currentTime);
        }

        /// <summary>
        /// Evaluates each room state
        /// </summary>
        /// <returns></returns>
        private async Task UpdateRooms()
        {
            foreach (var room in _rooms.Values)
            {
                await room.PeriodicUpdate();
            }
        }

        /// <summary>
        /// Check is we can resolve confused moves after next periodic check
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        private async Task ResolveConfusions(DateTimeOffset currentTime)
        {
            var toRemove = new HashSet<MotionVector>();

            foreach (var confusedVector in _confusedVectors)
            {
                // When timeout we have to delete confused vector
                if (currentTime.Between(confusedVector.End.TimeStamp).LastedLongerThen(_motionConfiguration.ConfusionResolutionTimeOut))
                {
                    toRemove.Add(confusedVector);
                    continue;
                }

                var startRoom = GetSourceRoom(confusedVector);
                var endRoom = GetTargetRoom(confusedVector);
                var startNeighbors = startRoom.NeighborsCache.ToList().AddChained(startRoom).RemoveChained(endRoom);

                if (currentTime.Between(confusedVector.Start.TimeStamp).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime))
                {
                    var noMoveInStartNeighbors = startNeighbors.All(n => n.LastMotion.Time.GetValueOrDefault() <= confusedVector.Start.TimeStamp);
                    if (noMoveInStartNeighbors)
                    {
                        await MarkVector(confusedVector.UnConfuze());
                        toRemove.Add(confusedVector);
                    }
                }
            }

            _confusedVectors.RemoveAll(toRemove.Contains);
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

            // If there was no confused vector from this point we don't start another
            //if (targetRoom.HasSameLastTimeVector(motionVector)) return;

            Logger.LogInformation(motionVector.ToString());

            await sourceRoom.MarkLeave(motionVector);
            targetRoom.MarkEnter(motionVector);
        }

        /// <summary>
        /// Check if two point in time can physically be a proper vector
        /// </summary>
        /// <param name="start"></param>
        /// <param name="potencialEnd"></param>
        /// <returns></returns>
        private bool IsProperVector(MotionPoint start, MotionPoint potencialEnd) => AreNeighbors(start, potencialEnd) && potencialEnd.IsMovePhisicallyPosible(start, _motionConfiguration.MotionMinDiff);

        /// <summary>
        /// CHeck if two points are neighbors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _rooms?[p1.Uid]?._neighbors?.Contains(p2.Uid) ?? false;

        /// <summary>
        /// Get all neighbors of given room
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        private IEnumerable<Room> GetNeighbors(string roomId) => _rooms.Where(x => _rooms[roomId]._neighbors.Contains(x.Key)).Select(y => y.Value);

        /// <summary>
        /// Get room pointed by end of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private Room GetTargetRoom(MotionVector motionVector) => _rooms[motionVector.End.Uid];

        /// <summary>
        /// Get room pointed by beginning of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private Room GetSourceRoom(MotionVector motionVector) => _rooms[motionVector.Start.Uid];

        /// <summary>
        /// Get room pointed by <paramref name="point"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private Room GetRoom(MotionWindow point) => _rooms[point.Start.Uid];

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
            return !_rooms[roomId].AutomationDisabled;
        }

        protected int Handle(NumberOfPeopleQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _rooms[roomId].NumberOfPersonsInArea;
        }

        protected AreaDescriptor Handle(AreaDescriptorQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _rooms[roomId].AreaDescriptor.Clone();
        }

        protected MotionStatus Handle(MotionServiceStatusQuery query)
        {
            return new MotionStatus
            {
                NumberOfPersonsInHouse = _rooms.Sum(md => md.Value.NumberOfPersonsInArea),
                NumberOfConfusions = _confusedVectors.Count
            };
        }
    }
}