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
using HomeCenter.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    //TODO Add time between rooms - people walks 6km/1h => 6000m/3600s => 1m = 600ms
    [ProxyCodeGenerator]
    public abstract class LightAutomationService : Service
    {
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly MotionConfiguration _motionConfiguration = new MotionConfiguration();

        private RoomService _roomService = null!; // Initialized in OnStart

        protected LightAutomationService(IConcurrencyProvider concurrencyProvider)
        {
            _concurrencyProvider = concurrencyProvider;
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context);

            try
            {
                ReadConfigurationFromProperties();
                var areas = ReadAreasFromAttachedProperties();
                ReadRoomsFromAttachedProperties(areas);

                StartWatchForEvents();
            }
            catch (Exception rr)
            {

                throw;
            }

           
        }

        private void ReadConfigurationFromProperties()
        {
            _motionConfiguration.MotionTimeWindow = this.AsTime(MotionProperties.MotionTimeWindow, TimeSpan.FromMilliseconds(3000));
            _motionConfiguration.ConfusionResolutionTime = this.AsTime(MotionProperties.ConfusionResolutionTime, TimeSpan.FromMilliseconds(5000));
            _motionConfiguration.ConfusionResolutionTimeOut = this.AsTime(MotionProperties.ConfusionResolutionTimeOut, TimeSpan.FromMilliseconds(10000));
            _motionConfiguration.MotionMinDiff = this.AsTime(MotionProperties.MotionMinDiff, TimeSpan.FromMilliseconds(500));
            _motionConfiguration.PeriodicCheckTime = this.AsTime(MotionProperties.PeriodicCheckTime, TimeSpan.FromMilliseconds(1000));
            _motionConfiguration.TurnOffTimeoutExtenderFactor = this.AsDouble(MotionProperties.TurnOffTimeoutIncrementPercentage, 50);
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

            CheckForMissingRooms();

            foreach (var motionDetector in ComponentsAttachedProperties)
            {
                var neighbours = new Lazy<IEnumerable<Room>>(() => from neighbor in motionDetector.AsList(MotionProperties.Neighbors)
                                                                   join room in rooms on neighbor equals room.Uid
                                                                   select room);

                rooms.Add(new Room(motionDetector.AttachedActor, neighbours, motionDetector.AsString(MotionProperties.Lamp),
                                    _concurrencyProvider, Logger, MessageBroker, areas.Get(motionDetector.AttachedArea, AreaDescriptor.Default), _motionConfiguration));
            }

            _roomService = new RoomService(rooms, _motionConfiguration);
        }

        private void CheckForMissingRooms()
        {
            var missingRooms = ComponentsAttachedProperties
                                    .SelectMany(n => n.AsList(MotionProperties.Neighbors))
                                    .Distinct()
                                    .Except(ComponentsAttachedProperties.Select(r => r.AttachedActor))
                                    .ToList();

            if (missingRooms.Count > 0) throw new ConfigurationException($"Following neighbors have not registered rooms: {string.Join(", ", missingRooms)}");
        }

        private void StartWatchForEvents()
        {
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
                                      .Select(move => new MotionWindow(move.Value.Message.MessageSource, move.Timestamp, _roomService));

            motionWindows.Subscribe(HandleMove, HandleError, Token, _concurrencyProvider.Task);

            motionWindows.Window(events, _ => Observable.Timer(_motionConfiguration.MotionTimeWindow, _concurrencyProvider.Scheduler))
                         .SelectMany(x => x.Scan((vectors, currentPoint) => vectors.AccumulateVector(currentPoint.Start))
                         .SelectMany(motion => motion.ToVectors()))
                         .GroupBy(room => room.EndPoint)
                         .SelectMany(r => r.Buffer(TimeSpan.FromMilliseconds(100), _concurrencyProvider.Scheduler)
                                          .Where(b => b.Count > 0)
                                    )
                         .Subscribe(HandleVectors, HandleError, Token, _concurrencyProvider.Task);
        }

        /// <summary>
        /// Handle move inside a room
        /// </summary>
        /// <param name="point"></param>
        private Task HandleVectors(IList<MotionVector> vectors) => _roomService.HandleVectors(vectors);

        /// <summary>
        /// Handle move inside a room
        /// </summary>
        /// <param name="motion"></param>
        private Task HandleMove(MotionWindow motion) => _roomService.MarkMotion(motion);

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
            await _roomService.UpdateRooms(currentTime);
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
                _roomService[roomId].DisableAutomation(command.AsTime(MessageProperties.TimeOut));
            }
            else
            {
                _roomService[roomId].DisableAutomation();
            }
        }

        protected void Handle(EnableAutomationCommand command)
        {
            var roomId = command.AsString(MotionProperties.RoomId);
            _roomService[roomId].EnableAutomation();
        }

        protected bool Handle(AutomationStateQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return !_roomService[roomId].AutomationDisabled;
        }

        protected int Handle(NumberOfPeopleQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _roomService[roomId].NumberOfPersons;
        }

        protected AreaDescriptor Handle(AreaDescriptorQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _roomService[roomId].AreaDescriptor.Clone();
        }

        protected MotionStatus Handle(MotionServiceStatusQuery query)
        {
            return new MotionStatus
            {
                NumberOfPersonsInHouse = _roomService.NumberOfPersons(),
                NumberOfConfusions = 0 //TODO
            };
        }
    }
}