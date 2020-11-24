using HomeCenter.Abstractions;
using HomeCenter.Conditions;
using HomeCenter.Model.Components;
using HomeCenter.Model.Triggers;
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
        private readonly ILogger<HomeCenter.Model.Components.ComponentProxy> _logger;

        public ComponentMapper(DeviceActorMapper actorMapper, BaseObjectMapper baseObjectMapper,
            IMessageBroker messageBroker, ILogger<HomeCenter.Model.Components.ComponentProxy> logger)
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

            var translators = config.Translators.Select(ar => new Translator(_baseObjectMapper.Map<ActorMessage>(ar.From), _baseObjectMapper.Map<ActorMessage>(ar.To)));

            //TODO conitions
            var triggers = config.Triggers.Select(ar => new Trigger(
                _baseObjectMapper.Map<Event>(ar.Event),
                ar.Commands.Select(c => _baseObjectMapper.Map<Command>(c)).ToList(),
                new Schedule(ar.Schedule.CronExpression, ar.Schedule.Calendar, ar.Schedule.WorkingTime, ar.Schedule.ManualSchedules.Select(schedule => new ManualSchedule(schedule.Start, schedule.Finish, schedule.WorkingTime)).ToList()),
                new ConditionContainer(ar.Condition.Expression, ar.Condition.IsInverted, ar.Condition.DefaultOperator, null))
            );

            var component = new ComponentProxy(adapterReferences, translators, triggers, _messageBroker, _logger);

            _actorMapper.Map(config, component);

            return component;
        }
    }
}