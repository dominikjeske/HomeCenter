using HomeCenter.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HomeCenter.Utils
{
    public static class AssemblyHelper
    {
        private const string TestAssembliesName = "Test";

        /// <summary>
        /// Get a list of all assemblies in solution
        /// </summary>
        /// <param name="ignoreTestAssemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetProjectAssemblies(bool ignoreTestAssemblies = true)
        {
            var mainAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var applicationNameName = mainAssemblyName.Substring(0, mainAssemblyName.IndexOf("."));
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var list = assemblies.Where(a => a.GetCustomAttribute<AssemblyProductAttribute>()?.Product?.IndexOf(applicationNameName) > -1);
            if (ignoreTestAssemblies)
            {
                list = list.Where(x => x.FullName.IndexOf(TestAssembliesName) == -1);
            }

            return list;
        }

        /// <summary>
        /// Get all types in solution accessible via type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllTypes<T>(bool inclueAbstract = false) => GetAllTypes(typeof(T), inclueAbstract);

        public static IEnumerable<Type> GetAllTypes(Type type, bool inclueAbstract = false)
        {
            var types = GetProjectAssemblies().SelectMany(s => s.GetTypes())
                                              .Where(p => type.IsAssignableFrom(p));

            if (!inclueAbstract)
            {
                types = types.Where(t => !t.IsAbstract);
            }

            return types;
        }

        public static IEnumerable<string> GetReferencedAssemblies(Type sourceType)
        {
            var allAsseblies = AppDomain.CurrentDomain.GetAssemblies();
            var referenced = sourceType.GetTypeInfo().Assembly.GetReferencedAssemblies().Select(a => a.FullName).ToList().AddChained(sourceType.GetTypeInfo().Assembly.FullName);

            return referenced.Select(assembly => Array.Find(allAsseblies, a => a.FullName == assembly)?.Location).Where(x => !string.IsNullOrWhiteSpace(x));
        }

        public static IEnumerable<Type> GetTypesWithAttribute<T>(bool inherit = false) 
        {
            var assembiles = GetProjectAssemblies();
            foreach (Type type in assembiles.SelectMany(a => a.GetTypes()))
            {
                if (type.GetCustomAttributes(typeof(T), inherit).Length > 0)
                {
                    yield return type;
                }
            }
        }
    }
}