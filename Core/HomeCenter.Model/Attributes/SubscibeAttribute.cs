using System;

namespace HomeCenter.Model.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SubscribeAttribute : Attribute
    {
    }
}