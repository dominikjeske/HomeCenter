using HomeCenter.Abstractions;
using HomeCenter.Services.Configuration.DTO;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Linq;

namespace HomeCenter.Services.Actors
{
    internal class ComponentMapper : ITypeMapper<ComponentDTO>
    {
        private readonly DeviceActorMapper _actorMapper;
        private readonly BaseObjectMapper _baseObjectMapper;
        private readonly IMessageBroker _messageBroker;
        private readonly ILogger<ComponentProxy> _logger;

        public ComponentMapper(DeviceActorMapper actorMapper, BaseObjectMapper baseObjectMapper,
            IMessageBroker messageBroker, ILogger<ComponentProxy> logger)
        {
            _actorMapper = actorMapper;
            _baseObjectMapper = baseObjectMapper;
            _messageBroker = messageBroker;
            _logger = logger;
        }

        public IActor Map(ComponentDTO config, Type destinationType)
        {
            var adapterReferences = config.AdapterReferences.Select(ar => new AdapterReference
            {
                IsMainAdapter = ar.IsMainAdapter,
                Type = ar.Type,
                Uid = ar.Uid
            }).ToList();

            var translators = config.Translators.Select(ar => new Translator
            {
                From = _baseObjectMapper.Map<ActorMessage>(ar.From),
                To = _baseObjectMapper.Map<ActorMessage>(ar.To)
            }).ToList();

            var triggers = config.Triggers.Select(ar => new Trigger
            {
                Commands = ar.Commands.Select(c => _baseObjectMapper.Map<Command>(c)).ToList(),
                Event = _baseObjectMapper.Map<Event>(ar.Event),
                Schedule = new Schedule
                {
                    Calendar = ar.Schedule.Calendar,
                    CronExpression = ar.Schedule.CronExpression,
                    WorkingTime = ar.Schedule.WorkingTime,
                    ManualSchedules = ar.Schedule.ManualSchedules.Select(schedule => new ManualSchedule
                    {
                        Start = schedule.Start,
                        Finish = schedule.Finish,
                        WorkingTime = schedule.WorkingTime
                    }).ToList(),
                },
                Condition = new ConditionContainer
                {
                    IsInverted = ar.Condition.IsInverted,
                    DefaultOperator = ar.Condition.DefaultOperator,
                    Expression = ar.Condition.Expression,

                    //TODO
                    //Conditions = ar.Condition.Conditions.Select(x =>
                }
            }).ToList();

            var component = new ComponentProxy(adapterReferences, translators, triggers, _messageBroker, _logger);

            _actorMapper.Map(config, component);

            return component;
        }
    }
}