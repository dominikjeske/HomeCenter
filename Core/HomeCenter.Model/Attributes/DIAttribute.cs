using System;

namespace HomeCenter.Model.Core
{
    /// <summary>
    /// Property injected in proxy generated class
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DIAttribute : Attribute
    {
    }
}