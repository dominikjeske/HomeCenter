using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Services.Actors
{
    public class ClassActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public ClassActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object Create(Type destinationType)
        {
            var paramsList = new List<object>();

            var ctor = destinationType.GetConstructors()
                                      .Select
                                      (
                                         x => new
                                         {
                                             Ctor = x,
                                             Parameters = x.GetParameters()
                                         }
                                      )
                                      .OrderByDescending(y => y.Parameters.Count())
                                      .Single();

            foreach (var par in ctor.Parameters)
            {
                var parInstance = _serviceProvider.GetService(par.ParameterType);
                paramsList.Add(parInstance);
            }

            var instance = ctor.Ctor.Invoke(paramsList.ToArray());

            return instance;
        }
    }
}