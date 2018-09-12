using Castle.DynamicProxy;
using HomeCenter.Core;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HomeCenter.Model.Extensions
{
    public static class ContainerExtensions
    {
        public static Registration RegisterService<T, K>(this Container container) where T : class, IService
                                                                                   where K : class, T
        {
            var registration = Lifestyle.Singleton.CreateRegistration<K>(container);
            container.AddRegistration<T>(registration);
            return registration;
        }

        public static Registration RegisterService<T>(this Container container, T instance) where T : class, IService
        {
            var registration = Lifestyle.Singleton.CreateRegistration<T>(() => instance, container);
            container.AddRegistration<T>(registration);
            return registration;
        }

        private static readonly ProxyGenerator generator = new ProxyGenerator();

        private static readonly Func<Type, object, IAsyncInterceptor, object[], object> createProxy =
               (p, t, i, par) => generator.CreateClassProxyWithTarget(p, t, par, i);

        public static void InterceptWith<TInterceptor>(this Container c, Predicate<Type> predicate) where TInterceptor : class, IAsyncInterceptor
        {
            c.ExpressionBuilt += (s, e) =>
            {
                if (predicate(e.RegisteredServiceType))
                {
                    var interceptorExpression = c.GetRegistration(typeof(TInterceptor), true).BuildExpression();

                    var tab = new List<Expression>();
                    foreach (var par in e.RegisteredServiceType.GetConstructors().FirstOrDefault().GetParameters())
                    {
                        var parInstance = c.GetRegistration(par.ParameterType, true).BuildExpression();
                        tab.Add(parInstance);
                    }

                    e.Expression = Expression.Convert(Expression.Invoke(Expression.Constant(createProxy),
                            Expression.Constant(e.RegisteredServiceType, typeof(Type)),
                            e.Expression,
                            interceptorExpression,
                            Expression.NewArrayInit(typeof(object), tab)),
                        e.RegisteredServiceType);
                }
            };
        }
    }
}