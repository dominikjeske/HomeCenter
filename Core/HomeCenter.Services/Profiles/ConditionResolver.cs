using AutoMapper;
using HomeCenter.Model.Conditions;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Utils;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Services.Profiles
{
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