using HomeCenter.Model.Actors;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Areas;
using HomeCenter.Model.Calendars;
using HomeCenter.Model.Components;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Triggers.Calendars;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.Networking;
using HomeCenter.Utils;
using HomeCenter.Utils.Extensions;
using Proto;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.Actors
{
    internal class ActorLoader : IActorLoader
    {
        //private MapperConfigurationExpression _mapperConfigurationExpression;
        //private IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _dynamicTypes = new Dictionary<string, Type>();
        private readonly IScheduler _scheduler;
        private readonly IEnumerable<ITypeMapper> _typeMappers;

        public async Task LoadTypes()
        {
            LoadDynamicTypes();

            //_mapper = RegisterTypesInAutomapper();
            await LoadCalendars();
        }

        public ActorLoader(IServiceProvider serviceProvider, IScheduler scheduler, IEnumerable<ITypeMapper> typeFactories)
        {
            _serviceProvider = serviceProvider;
            _scheduler = scheduler;
            _typeMappers = typeFactories;
        }

        private void LoadDynamicTypes()
        {
            // force to load HomeCenter.ActorsContainer into memory
            var testAdapter = typeof(HomeCenter.Actors.ForceAssemblyLoadType);

            AssemblyHelper.GetAllTypes<Adapter>(false).ForEach(type => _dynamicTypes.Add(type.Name, type));
            AssemblyHelper.GetAllTypes<Service>(false).ForEach(type => _dynamicTypes.Add(type.Name, type));
            AssemblyHelper.GetAllTypes<Area>(false).ForEach(type => _dynamicTypes.Add(type.Name, type));
            AssemblyHelper.GetAllTypes<Component>(false).ForEach(type => _dynamicTypes.Add(type.Name, type));
        }

        private async Task LoadCalendars()
        {
            foreach (var calendarType in AssemblyHelper.GetAllTypes<IDayOffProvider>())
            {
                var dayOffProvider = calendarType.CreateInstance<IDayOffProvider>();
                var calendar = new QuartzCalendar(dayOffProvider);

                await _scheduler.AddCalendar(dayOffProvider.Name, calendar, false, false);
            }
        }

        public IActor GetProxyType<T>(T actorConfig) where T : IBaseObject
        {
            var proxyName = $"{actorConfig.Type}Proxy";
            if (!_dynamicTypes.ContainsKey(proxyName)) throw new ConfigurationException($"Could not find type for actor {proxyName}");
            var actorType = _dynamicTypes[proxyName];

            if (_typeMappers.FirstOrDefault(type => type is ITypeMapper<T>) is not ITypeMapper<T> mapper)
            {
                //throw new ConfigurationException($"Could not find mapper for {typeof(T).Name}");
                return null;
            }

            
            try
            {

                var actor = mapper.Create(actorConfig, actorType);
                return actor;
                

                
            }
            catch (Exception ee)
            {

                throw;
            }
            
        }

        //private IMapper RegisterTypesInAutomapper()
        //{
        //    _mapperConfigurationExpression = new MapperConfigurationExpression();
        //    _mapperConfigurationExpression.ConstructServicesUsing(_serviceProvider.GetService);

        //    foreach (var profile in AssemblyHelper.GetAllTypes<Profile>())
        //    {
        //        _mapperConfigurationExpression.AddProfile(profile);
        //    }

        //    _mapperConfigurationExpression.ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);

        //    foreach (var actorType in _dynamicTypes.Values)
        //    {
        //        if (typeof(Adapter).IsAssignableFrom(actorType))
        //        {
        //            _mapperConfigurationExpression.CreateMap(typeof(AdapterDTO), actorType).ConstructUsingServiceLocator();
        //        }
        //        else if (typeof(Service).IsAssignableFrom(actorType))
        //        {
        //            _mapperConfigurationExpression.CreateMap(typeof(ServiceDTO), actorType).ConstructUsingServiceLocator();
        //        }
        //    }

        //    var mapper = new Mapper(new MapperConfiguration(_mapperConfigurationExpression), t => _serviceProvider.GetService(t));

        //    return mapper;
        //}
    }
}