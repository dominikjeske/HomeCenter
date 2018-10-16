using AutoMapper;
using HomeCenter.Model.Areas;
using HomeCenter.Model.Components;
using HomeCenter.Model.Conditions;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Triggers;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Utils;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Services.DI
{
    public class HomeCenterMappingProfile : Profile
    {
        public HomeCenterMappingProfile()
        {
            ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);

            CreateMap<ComponentDTO, ComponentProxy>().ConstructUsingServiceLocator();
            CreateMap<AdapterReferenceDTO, AdapterReference>();
            CreateMap<TriggerDTO, Trigger>().ForMember(s => s.Commands, d => d.ResolveUsing<CommandResolver>()).ConstructUsingServiceLocator();
            CreateMap<EventDTO, Event>();
            CreateMap<AreaDTO, Area>();
            CreateMap<ScheduleDTO, Schedule>();
            CreateMap<ConditionContainerDTO, ConditionContainer>().ForMember(s => s.Conditions, d => d.ResolveUsing<ConditionResolver>()).ConstructUsingServiceLocator();

            var commandTypes = AssemblyHelper.GetAllTypes<Command>();
            foreach (var commandType in commandTypes)
            {
                CreateMap(typeof(CommandDTO), commandType);
            }
        }
    }


    public class CommandResolver : IValueResolver<TriggerDTO, Trigger, IList<Command>>
    {
        public IList<Command> Resolve(TriggerDTO source, Trigger destination, IList<Command> destMember, ResolutionContext context)
        {
            var commands = new List<Command>();
            var types = AssemblyHelper.GetAllTypes<Command>();

            foreach (var commandDTO in source.Commands)
            {
                var commandType = types.FirstOrDefault(c => c.Name.IndexOf(commandDTO.Type) > -1);
                var command = context.Mapper.Map(commandDTO, commandDTO.GetType(), commandType) as Command;
                commands.Add(command);
            }

            return commands;
        }
    }

    public class ConditionResolver : IValueResolver<ConditionContainerDTO, ConditionContainer, IList<IValidable>>
    {
        public IList<IValidable> Resolve(ConditionContainerDTO source, ConditionContainer destination, IList<IValidable> destMember, ResolutionContext context)
        {
            IList<IValidable> list = new List<IValidable>();
            var types = AssemblyHelper.GetAllTypes<IValidable>();

            foreach (var condition in source.Conditions)
            {
                var conditionType = types.FirstOrDefault(c => c.Name.IndexOf(condition.Type) > -1);

                var cons = conditionType.GetConstructors().FirstOrDefault();
                var args = new List<object>();

                foreach (var parameter in cons.GetParameters())
                {
                    args.Add(context.ConfigurationProvider.ServiceCtor(parameter.ParameterType));
                }

                if (conditionType != null)
                {
                    list.Add((IValidable)cons.Invoke(args.ToArray()));
                }
            }

            return list;
        }
    }
}