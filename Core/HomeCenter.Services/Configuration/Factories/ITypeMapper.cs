using HomeCenter.Model.Core;
using Proto;
using System;

namespace HomeCenter.Services.Actors
{
    internal interface ITypeMapper<TConfig> : ITypeMapper //where TConfig : IBaseObject
    {
        IActor Create(TConfig config, Type destinationType);
    }

    internal interface ITypeMapper
    {

    }
}