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

        private readonly ILogger<ComponentProxy> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ComponentMapper(DeviceActorMapper actorMapper, BaseObjectMapper baseObjectMapper,
            IServiceProvider serviceProvider, ILogger<ComponentProxy> logger)
        {
            _actorMapper = actorMapper;
            _baseObjectMapper = baseObjectMapper;
            _logger = logger;
            _serviceProvider = serviceProvider;
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

            var broker = _serviceProvider.Get<IMessageBroker>();

            var component = new ComponentProxy(adapterReferences, translators, triggers, broker, _logger);

            _actorMapper.Map(config, component);

            return component;
        }
    }
}