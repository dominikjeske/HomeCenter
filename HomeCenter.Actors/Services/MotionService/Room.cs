using CSharpFunctionalExtensions;
using HomeCenter.Abstractions;
using HomeCenter.Conditions;
using HomeCenter.Conditions.Specific;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class Room : IDisposable
    {
        private readonly ConditionContainer _turnOnConditionsValidator = new();
        private readonly ConditionContainer _turnOffConditionsValidator = new();
        private readonly DisposeContainer _disposeContainer = new();
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly ILogger _logger;
        private readonly IMessageBroker _messageBroker;
        private readonly string _lamp;
        private readonly ConfusedVectors ConfusedVectors;
        private readonly Lazy<RoomDictionary> _roomDictionary;
        private DateTimeOffset? _scheduledAutomationTime;

        internal RoomStatistic RoomStatistic { get; }
        internal string Uid { get; }
        internal AreaDescriptor AreaDescriptor { get; }
        internal bool AutomationDisabled { get; private set; }
        internal int NumberOfPersons => RoomStatistic.NumberOfPersons;

        public override string ToString() => $"{Uid} [Last move: {RoomStatistic.LastMotion}] [Persons: {NumberOfPersons}]";

        public Room(string uid, string lamp, IConcurrencyProvider concurrencyProvider, ILogger<Room> logger, Lazy<RoomDictionary> roomDictionary,
                    IMessageBroker messageBroker, AreaDescriptor areaDescriptor)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            _lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));
            _logger = new ContextLogger(logger, () =>
            {
                return new Dictionary<string, object>
                {
                    ["@Statistics"] = RoomStatistic!,
                    ["Room"] = Uid
                };
            });

            _concurrencyProvider = concurrencyProvider;
            _messageBroker = messageBroker;
            _roomDictionary = roomDictionary;
            AreaDescriptor = areaDescriptor;

            if (areaDescriptor.WorkingTime == WorkingTime.DayLight)
            {
                _turnOnConditionsValidator.WithCondition(new IsDayCondition(_messageBroker));
            }
            else if (areaDescriptor.WorkingTime == WorkingTime.AfterDusk)
            {
                _turnOnConditionsValidator.WithCondition(new IsNightCondition(_messageBroker));
            }

            _turnOnConditionsValidator.WithCondition(new IsEnabledAutomationCondition(this));
            _turnOffConditionsValidator.WithCondition(new IsEnabledAutomationCondition(this));
            _turnOffConditionsValidator.WithCondition(new IsTurnOffAutomaionCondition(this));

            RoomStatistic = new RoomStatistic(_logger, AreaDescriptor);
            ConfusedVectors = new ConfusedVectors(_logger, Uid, AreaDescriptor.Motion.ConfusionResolutionTime,
               AreaDescriptor.Motion.ConfusionResolutionTimeOut, _roomDictionary);

            _disposeContainer.Add(RoomStatistic.ProbabilityChange.SelectMany(ProbalityChange).Subscribe());
            _disposeContainer.Add(ConfusedVectors.Resolved.Subscribe(VectorResolved));

            RegisterChangeStateSource();
        }

        private async Task<Unit> ProbalityChange(Probability probability)
        {
            await TryChangeLampState(probability);
            return Unit.Default;
        }

        private void VectorResolved(MotionVector vector)
        {
            MarkVector(vector, true);
        }

        private void RegisterChangeStateSource()
        {
            //TODO
            //_disposeContainer.Add(Lamp.PowerStateChange.Subscribe(PowerStateChangeHandler));
        }

        /// <summary>
        /// Take action when there is a move in the room
        /// </summary>
        public void MarkMotion(DateTimeOffset motionTime)
        {
            RoomStatistic.MarkMotion(motionTime);
        }

        /// <summary>
        /// Update room state on time intervals
        /// </summary>
        public void PeriodicUpdate(DateTimeOffset motionTime)
        {
            CheckForScheduledAutomation(motionTime);
            RoomStatistic.RecalculateProbability(motionTime);
        }

        /// <summary>
        /// Handle vector candidates that occurred in room
        /// </summary>
        public void HandleVectors(IList<MotionVector> motionVectors)
        {
            if (motionVectors.Count == 0) return;

            // When we have one vector we know that there is no concurrent vectors to same room
            if (motionVectors.Count == 1)
            {
                var vector = motionVectors.Single();

                if (IsTurnOnVector(vector))
                {
                    MarkVector(vector, false);
                }
                else
                {
                    ConfusedVectors.MarkConfusion(new MotionVector[] { vector });
                }
            }
            // When we have at least two vectors we know that we have confusion
            else
            {
                ConfusedVectors.MarkConfusion(motionVectors);
            }
        }

        public void EvaluateConfusions(DateTimeOffset dateTimeOffset) => ConfusedVectors.EvaluateConfusions(dateTimeOffset);

        public void EnableAutomation()
        {
            AutomationDisabled = false;

            _logger.LogInformation(MoveEventId.AutomationEnabled, "Automation enabled");
        }

        public void DisableAutomation(TimeSpan time)
        {
            AutomationDisabled = true;

            _logger.LogInformation(MoveEventId.AutomationDisabled, "Automation disabled");

            if (time != TimeSpan.Zero)
            {
                _scheduledAutomationTime = _concurrencyProvider.Scheduler.Now + time;
            }
        }

        public void Dispose() => _disposeContainer.Dispose();

        /// <summary>
        /// Check if <paramref name="motionVector"/> is vector that turned on the light
        /// </summary>
        private bool IsTurnOnVector(MotionVector motionVector) => !RoomStatistic.FirstEnterTime.HasValue || RoomStatistic.FirstEnterTime == motionVector.EndTime;

        /// <summary>
        /// Marks enter to target room and leave from source room
        /// </summary>
        private void MarkVector(MotionVector motionVector, bool resolved)
        {
            _roomDictionary.Value[motionVector.StartPoint].MarkLeave(motionVector);
            MarkEnter(motionVector);

            _logger.LogInformation(MoveEventId.MarkVector, "{vector} changed with {VectorStatus}", motionVector, resolved ? "Resolved" : "Normal");
        }

        /// <summary>
        /// Marks entrance of last motion vector
        /// </summary>
        private void MarkEnter(MotionVector vector) => RoomStatistic.MarkEnter(vector.EndTime);

        private void MarkLeave(MotionVector vector) => RoomStatistic.MarkLeave(vector);

        private async Task TryChangeLampState(Probability probability)
        {
            if (probability.IsFullProbability)
            {
                await TryTurnOnLamp();
            }
            else if (probability.IsNoProbability)
            {
                await TryTurnOffLamp();
            }
        }

        private void PowerStateChangeHandler(PowerStateChangeEvent powerChangeEvent)
        {
            if (!powerChangeEvent.Value)
            {
                RoomStatistic.Reset();
            }

            _logger.LogDebug(MoveEventId.PowerState, "{newState} | Source: {source}", powerChangeEvent.Value, powerChangeEvent.EventTriggerSource);
        }

        private void CheckForScheduledAutomation(DateTimeOffset motionTime)
        {
            if (AutomationDisabled && motionTime > _scheduledAutomationTime)
            {
                EnableAutomation();
            }
        }

        private async Task TryTurnOnLamp()
        {
            if (await _turnOnConditionsValidator.Validate())
            {
                _logger.LogInformation(MoveEventId.PowerState, "Turning ON");

                _messageBroker.Send(new TurnOnCommand(), _lamp);
            }
        }

        private async Task TryTurnOffLamp()
        {
            if (await _turnOffConditionsValidator.Validate())
            {
                _logger.LogInformation(MoveEventId.PowerState, "Turning OFF");

                _messageBroker.Send(new TurnOffCommand(), _lamp);

                RoomStatistic.SetAutoTurnOffTime(_concurrencyProvider.Scheduler.Now);
            }
        }
    }
}