using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.Model.Extensions
{
    public static class Extensions
    {
        public static IEnumerable<MethodInfo> GetMethodsBySignatureWithReturnType(this Type type, Type returnType, params Type[] parameterTypes)
        {
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where((m) =>
            {
                if (m.ReturnType != returnType) return false;
                var parameters = m.GetParameters();
                if ((parameterTypes == null || parameterTypes.Length == 0))
                    return parameters.Length == 0;
                if (parameters.Length != parameterTypes.Length)
                    return false;
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameters[i].ParameterType != parameterTypes[i])
                        return false;
                }
                return true;
            });
        }

        public static IEnumerable<MethodInfo> GetMethodsBySignature(this Type type, params Type[] parameterTypes)
        {
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where((m) =>
            {
                var parameters = m.GetParameters();
                if ((parameterTypes == null || parameterTypes.Length == 0))
                    return parameters.Length == 0;
                if (parameters.Length != parameterTypes.Length)
                    return false;
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameters[i].ParameterType != parameterTypes[i])
                        return false;
                }
                return true;
            });
        }

        public static Func<Command, Task<object>> WrapTaskToGenericTask(this MethodInfo handler, object objectInstance)
        {
            if
            (
                   handler.ReturnType.BaseType != typeof(Task)
                || handler.ReturnType.GetGenericArguments()?.Length != 1
                || handler.GetParameters().FirstOrDefault()?.ParameterType != typeof(Command)
            )
            {
                throw new Exception($"Input method for {nameof(WrapTaskToGenericTask)} should have following syntax: Task<ReturnType> Method(Command command)");
            }

            var commandParameter = Expression.Parameter(typeof(Command), "commandParameter");
            var task = Expression.Call(Expression.Constant(objectInstance), handler, commandParameter);
            if (task.Type != typeof(Task<object>))
            {
                task = Expression.Call(typeof(TaskExtensions), "ToGenericTaskResult", task.Type.GetGenericArguments(), task);
            }
            return Expression.Lambda<Func<Command, Task<object>>>(task, commandParameter).Compile();
        }

        public static Func<Command, Task<object>> WrapSimpleTypeToGenericTask(this MethodInfo handler, object objectInstance)
        {
            if
            (
                  handler.ReturnType.BaseType == typeof(Task)
               || handler.ReturnType == typeof(Task)
               || handler.ReturnType == typeof(void)
               || handler.GetParameters().FirstOrDefault()?.ParameterType != typeof(Command)
            )
            {
                throw new Exception($"Input method for {nameof(WrapSimpleTypeToGenericTask)} should have following syntax: ReturnType Method(Command command)");
            }

            var commandParameter = Expression.Parameter(typeof(Command), "commandParameter");
            var result = Expression.Call(Expression.Constant(objectInstance), handler, commandParameter);
            result = Expression.Call(typeof(Task), "FromResult", new Type[] { typeof(object) }, Expression.Convert(result, typeof(object)));
            return Expression.Lambda<Func<Command, Task<object>>>(result, commandParameter).Compile();
        }

        public static Func<Command, Task<object>> WrapReturnTypeToGenericTask(this MethodInfo handler, object objectInstance)
        {
            var commandParameter = Expression.Parameter(typeof(Command), "commandParameter");
            var result = Expression.Call(Expression.Constant(objectInstance), handler, commandParameter);
            result = Expression.Call(typeof(Task), "FromResult", new Type[] { typeof(object) }, Expression.Convert(result, typeof(object)));
            return Expression.Lambda<Func<Command, Task<object>>>(result, commandParameter).Compile();
        }
    }
}