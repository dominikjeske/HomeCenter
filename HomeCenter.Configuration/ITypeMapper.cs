using Proto;
using System;

namespace HomeCenter.Services.Actors
{
    public interface ITypeMapper<TConfig> : ITypeMapper
    {
        IActor Map(TConfig config, Type destinationType);
    }

    public interface ITypeMapper
    {
    }
}