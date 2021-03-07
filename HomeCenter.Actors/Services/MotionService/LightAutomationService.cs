using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Extensions;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Queries.Services;
using HomeCenter.Model.Extensions;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    //TODO Add time between rooms - people walks 6km/1h => 6000m/3600s => 1m = 600ms
    [Proxy]
    public class LightAutomationService : Service
    {
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly ILoggerFactory _loggerFactory;
        private MotionConfiguration? _motionConfiguration;
        private RoomDictionary _roomDictionary = null!; // Initialized in OnStart

        protected LightAutomationService(IConcurrencyProvider concurrencyProvider, ILoggerFactory loggerFactory)
        {
            _concurrencyProvider = concurrencyProvider;
            _loggerFactory = loggerFactory;
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
            _motionConfiguration = new MotionConfiguration
            {
                MotionTimeWindow = this.AsTime(MotionProperties.MotionTimeWindow, MotionDefaults.MotionTimeWindow),
                ConfusionResolutionTime = this.AsTime(MotionProperties.ConfusionResolutionTime, MotionDefaults.ConfusionResolutionTime),
                ConfusionResolutionTimeOut = this.AsTime(MotionProperties.ConfusionResolutionTimeOut, MotionDefaults.ConfusionResolutionTimeOut),
                MotionMinDiff = this.AsTime(MotionProperties.MotionMinDiff, MotionDefaults.MotionMinDiff),
                PeriodicCheckTime = this.AsTime(MotionProperties.PeriodicCheckTime, MotionDefaults.PeriodicCheckTime),
                TurnOffTimeoutExtenderFactor = this.AsDouble(MotionProperties.TurnOffTimeoutIncrementPercentage, MotionDefaults.TurnOffTimeoutExtenderFactor),
                DecreaseLeavingFactor = this.AsDouble(MotionProperties.TurnOffTimeoutIncrementPercentage, MotionDefaults.DecreaseLeavingFactor),
                TurnOffTimeout = this.AsTime(MotionProperties.TurnOffTimeout, MotionDefaults.TurnOffTimeOut)
            };
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
                    LightIntensityAtNight = area.ContainsProperty(MotionProperties.LightIntensityAtNight) ? area.AsDouble(MotionProperties.LightIntensityAtNight) : null,
                    TurnOffTimeout = area.AsTime(MotionProperties.TurnOffTimeout, _motionConfiguration?.TurnOffTimeout),
                    TurnOffAutomationDisabled = area.AsBool(MotionProperties.TurnOffAutomationDisabled, false),
                    Motion = _motionConfiguration!
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
                var areaDescriptor = areas.Get(motionDetector.AttachedArea, AreaDescriptor.Default);

                rooms.Add(new Room(motionDetector.AttachedActor, motionDetector.AsString(MotionProperties.Lamp),
                                    _concurrencyProvider, _loggerFactory, new Lazy<RoomDictionary>(() => _roomDictionary), MessageBroker, areaDescriptor));
            }

            var neighbours = ComponentsAttachedProperties.SelectMany(r => r.AsList(MotionProperties.Neighbors).Select(n => new { Uid = r.AttachedActor, Neighbor = n }))
                                        .Join(rooms, o => o.Neighbor, i => i.Uid, (o, i) => new { o.Uid, Room = i })
                                        .GroupBy(g => g.Uid)
                                        .ToDictionary(x => x.Key, v => v.GroupBy(g => g.Room.Uid).ToDictionary(k => k.Key, v => v.Select(o => o.Room).FirstOrDefault()).AsReadOnly()).AsReadOnly();

            _roomDictionary = new RoomDictionary(rooms, neighbours, _motionConfiguration!);
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
        /// Checks periodically all rooms for current state - parallel to AnalyzeMove
        /// </summary>
        private void PeriodicCheck()
        {
            Observable.Interval(_motionConfiguration!.PeriodicCheckTime, _concurrencyProvider.Scheduler)
                      .Timestamp(_concurrencyProvider.Scheduler)
                      .Select(time => time.Timestamp)
                      .ObserveOn(_concurrencyProvider.Task)
                      .Subscribe(PeriodicCheck, HandleError);
        }

        /// <summary>
        /// Query motion events from system and analyses move
        /// </summary>
        private void AnalyzeMove()
        {
            var events = MessageBroker.Observe<MotionEvent>();

            var motionWindows = events.Timestamp(_concurrencyProvider.Scheduler)
                                      .Select(move => new MotionWindow(move.Value.Message.MessageSource, move.Timestamp, _roomDictionary));

            motionWindows.Subscribe(HandleMove, HandleError, Token);

            motionWindows.Window(events, _ => Observable.Timer(_motionConfiguration!.MotionTimeWindow, _concurrencyProvider.Scheduler))
                         .SelectMany(x => x.Scan((vectors, currentPoint) => vectors.AccumulateVector(currentPoint.Start))
                         .SelectMany(motion => motion.ToVectors()))
                         .GroupBy(room => room.EndPoint)
                         .SelectMany(r => r.Buffer(TimeSpan.FromMilliseconds(100), _concurrencyProvider.Scheduler)
                                          .Where(b => b.Count > 0)
                                    )
                         .Subscribe(HandleVectors, HandleError, Token);
        }

        private void HandleVectors(IList<MotionVector> vectors) => _roomDictionary.HandleVectors(vectors);

        private void HandleMove(MotionWindow motion) => _roomDictionary.MarkMotion(motion);

        private void HandleError(Exception ex) => Logger.LogError(ex, "Exception in LightAutomationService");

        private void PeriodicCheck(DateTimeOffset currentTime) => _roomDictionary.CheckRooms(currentTime);

        protected bool Handle(IsAliveQuery isAliveQuery) => true;

        protected void Handle(DisableAutomationCommand command)
        {
            var roomId = command.AsString(MotionProperties.RoomId);
            _roomDictionary[roomId].DisableAutomation(command.AsTime(MessageProperties.TimeOut, TimeSpan.Zero));
        }

        protected void Handle(EnableAutomationCommand command)
        {
            var roomId = command.AsString(MotionProperties.RoomId);
            _roomDictionary[roomId].EnableAutomation();
        }

        protected bool Handle(AutomationStateQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return !_roomDictionary[roomId].AutomationDisabled;
        }

        protected int Handle(NumberOfPeopleQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _roomDictionary[roomId].NumberOfPersons;
        }

        protected AreaDescriptor Handle(AreaDescriptorQuery query)
        {
            var roomId = query.AsString(MotionProperties.RoomId);
            return _roomDictionary[roomId].AreaDescriptor.Clone();
        }

        protected MotionStatus Handle(MotionServiceStatusQuery query)
        {
            return new MotionStatus
            {
                NumberOfPersonsInHouse = _roomDictionary.NumberOfPersons(),
                NumberOfConfusions = 0 //TODO
            };
        }
    }
}