using HomeCenter.Abstractions;
using HomeCenter.Assemblies;
using HomeCenter.Extensions;
using Proto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using HomeCenter.Actors.Core;
using HomeCenter.Model.Areas;

namespace HomeCenter.Services.Actors
{
    public class ActorLoader : IActorLoader
    {
        private readonly Dictionary<string, Type> _dynamicTypes = new Dictionary<string, Type>();
        private readonly IEnumerable<ITypeMapper> _typeMappers;

        public async Task LoadTypes()
        {
            LoadDynamicTypes();
        }

        public ActorLoader(IEnumerable<ITypeMapper> typeFactories)
        {
            _typeMappers = typeFactories;
        }

        private void LoadDynamicTypes()
        {
            //TODO DNF
            // force to load HomeCenter.ActorsContainer into memory
            //var testAdapter = typeof(HomeCenter.Actors.ForceAssemblyLoadType);

            AssemblyHelper.GetAllTypes<Adapter>(false).ForEach(type => _dynamicTypes.Add(type.Name, type));
            AssemblyHelper.GetAllTypes<Service>(false).ForEach(type => _dynamicTypes.Add(type.Name, type));
            AssemblyHelper.GetAllTypes<Area>(false).ForEach(type => _dynamicTypes.Add(type.Name, type));
            AssemblyHelper.GetAllTypes<Component>(false).ForEach(type => _dynamicTypes.Add(type.Name, type));
        }

        public IActor GetProxyType<T>(T actorConfig) where T : IBaseObject
        {
            var proxyName = $"{actorConfig.Type}Proxy";
            if (!_dynamicTypes.ContainsKey(proxyName)) throw new ConfigurationException($"Could not find type for actor {proxyName}");
            var actorType = _dynamicTypes[proxyName];

            if (_typeMappers.FirstOrDefault(type => type is ITypeMapper<T>) is not ITypeMapper<T> mapper)
            {
                throw new ConfigurationException($"Could not find mapper for {typeof(T).Name}");
            }

            var actor = mapper.Map(actorConfig, actorType);
            return actor;
        }
    }
}