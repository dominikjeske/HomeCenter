using AutoMapper;
using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.ComponentModel.Events;
using Wirehome.Conditions;
using Wirehome.Core.ComponentModel.Areas;
using Wirehome.Core.ComponentModel.Configuration;
using Wirehome.Core.Utils;

namespace Wirehome.Core.Services.DependencyInjection
{
    public class WirehomeMappingProfile : Profile
    {
        public WirehomeMappingProfile()
        {
            ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);

            CreateMap<ComponentDTO, Component>().ConstructUsingServiceLocator();
            CreateMap<AdapterReferenceDTO, AdapterReference>();
            CreateMap<TriggerDTO, Trigger>();
            CreateMap<CommandDTO, Command>();
            CreateMap<EventDTO, Event>();
            CreateMap<AreaDTO, Area>();
            CreateMap<ScheduleDTO, Schedule>();
            CreateMap<ConditionContainerDTO, ConditionContainer>().ForMember(s => s.Conditions, d => d.ResolveUsing<ConditionResolver>()).ConstructUsingServiceLocator();
        }
    }

    public class ConditionResolver : IValueResolver<ConditionContainerDTO, ConditionContainer, IList<IValidable>>
    {
        public IList<IValidable> Resolve(ConditionContainerDTO source, ConditionContainer destination, IList<IValidable> destMember, ResolutionContext context)
        {
            IList<IValidable> list = new List<IValidable>();
            var types = AssemblyHelper.GetProjectAssemblies()
                                      .SelectMany(s => s.GetTypes())
                                      .Where(p => typeof(IValidable).IsAssignableFrom(p));

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