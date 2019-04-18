using AutoMapper;
using HomeCenter.Model.Conditions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Triggers;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Utils;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Services.Profiles
{

    public class ConditionContainerResolver : IValueResolver<TriggerDTO, Trigger, IValidable>
    {
        public IValidable Resolve(TriggerDTO source, Trigger destination, IValidable destMember, ResolutionContext context)
        {
            if (source.Condition != null)
            {
                return context.Mapper.Map(source.Condition, typeof(ConditionContainerDTO), typeof(ConditionContainer)) as IValidable;
            }
            return EmptyCondition.Default;
        }
    }
}