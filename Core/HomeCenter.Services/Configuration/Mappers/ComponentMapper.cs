using HomeCenter.Model.Components;
using HomeCenter.Model.Conditions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Triggers;
using HomeCenter.Services.Configuration.DTO;
using Proto;
using System;
using System.Linq;

namespace HomeCenter.Services.Actors
{
    internal class ComponentMapper : ITypeMapper<ComponentDTO>
    {
        private readonly DeviceActorMapper _actorMapper;
        private readonly BaseObjectMapper _baseObjectMapper;

        public ComponentMapper(DeviceActorMapper actorMapper, BaseObjectMapper baseObjectMapper)
        {
            _actorMapper = actorMapper;
            _baseObjectMapper = baseObjectMapper;
        }

        public IActor Map(ComponentDTO config, Type destinationType)
        {
            var component = new Component
            {
                AdapterReferences = config.AdapterReferences.Select(ar => new AdapterReference
                {
                    IsMainAdapter = ar.IsMainAdapter,
                    Type = ar.Type,
                    Uid = ar.Uid
                }).ToList(),

                Translators = config.Translators.Select(ar => new Translator
                {
                    From = _baseObjectMapper.Map<ActorMessage>(ar.From),
                    To = _baseObjectMapper.Map<ActorMessage>(ar.To)
                }).ToList(),

                Triggers = config.Triggers.Select(ar => new Trigger
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
                }).ToList()
            };

            _actorMapper.Map(config, component);

            return component;
        }
    }
}