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
        private readonly Lazy<RoomDictionary> _roomDictionary;
        private DateTimeOffset? _scheduledAutomationTime;

        internal MotionEngine MotionEngine { get; }
        internal string Uid { get; }
        internal AreaDescriptor AreaDescriptor { get; }
        internal bool AutomationDisabled { get; private set; }
        internal int NumberOfPersons => MotionEngine.NumberOfPersons;

        public override string ToString() => $"{Uid} [Last move: {MotionEngine.LastMotion}] [Persons: {NumberOfPersons}]";

        public Room(string uid, string lamp, IConcurrencyProvider concurrencyProvider, ILogger<Room> logger, Lazy<RoomDictionary> roomDictionary,
                    IMessageBroker messageBroker, AreaDescriptor areaDescriptor)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            _lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));
            _logger = new ContextLogger(logger, () =>
            {
                return new Dictionary<string, object>
                {
                    ["@Statistics"] = MotionEngine!,
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

            MotionEngine = new MotionEngine(_logger, AreaDescriptor, Uid, _roomDictionary);

            _disposeContainer.Add(MotionEngine.ProbabilityChange.SelectMany(ProbalityChange).Subscribe());

            RegisterChangeStateSource();
        }

        private async Task<Unit> ProbalityChange(Probability probability)
        {
            await TryChangeLampState(probability);
            return Unit.Default;
        }

        private void RegisterChangeStateSource()
        {
            //TODO
            //_disposeContainer.Add(Lamp.PowerStateChange.Subscribe(PowerStateChangeHandler));
        }

        /// <summary>
        /// Take action when there is a move in the room
        /// </summary>
        public void MarkMotion(DateTimeOffset motionTime) => MotionEngine.MarkMotion(motionTime);

        /// <summary>
        /// Update room state on time intervals
        /// </summary>
        public void PeriodicUpdate(DateTimeOffset motionTime)
        {
            CheckForScheduledAutomation(motionTime);
            MotionEngine.RecalculateProbability(motionTime);
        }

        /// <summary>
        /// Handle vector candidates that occurred in room
        /// </summary>
        public void HandleVectors(IList<MotionVector> motionVectors) => MotionEngine.HandleVectors(motionVectors);

        public void EvaluateConfusions(DateTimeOffset dateTimeOffset) => MotionEngine.EvaluateConfusions(dateTimeOffset);

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

        public void MarkLeave(MotionVector vector) => MotionEngine.MarkLeave(vector);

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
                MotionEngine.Reset();
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

                MotionEngine.SetAutoTurnOffTime(_concurrencyProvider.Scheduler.Now);
            }
        }
    }
}