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