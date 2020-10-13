using Proto;
using System;

namespace HomeCenter.Services.Actors
{
    internal interface ITypeMapper<TConfig> : ITypeMapper
    {
        IActor Create(TConfig config, Type destinationType);
    }

    internal interface ITypeMapper
    {
    }
}