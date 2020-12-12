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
                                         constructor => new
                                         {
                                             Constructor = constructor,
                                             Parameters = constructor.GetParameters()
                                         }
                                      )
                                      .OrderByDescending(y => y.Parameters.Length)
                                      .Single();

            foreach (var par in ctor.Parameters)
            {
                var parInstance = _serviceProvider.GetService(par.ParameterType);

                if (parInstance is null) throw new InvalidOperationException($"Type not registered {par.ParameterType.FullName}");

                if (parInstance != null)
                {
                    paramsList.Add(parInstance);
                }
            }

            var instance = ctor.Constructor.Invoke(paramsList.ToArray());

            return instance;
        }
    }
}