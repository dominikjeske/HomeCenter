using Proto;
using System;

namespace HomeCenter.Services.Actors
{
    internal interface ITypeMapper<TConfig> : ITypeMapper
    {
        IActor Map(TConfig config, Type destinationType);
    }

    internal interface ITypeMapper
    {
    }
}