# Project desctiption

This project is .NET Core 2.1 application for Raspberry Pi 2/3 and Windows IoT and the main goal is to automate all devices in home into one HomeCenter.

# Core elements

- All objects in model are based on **BaseObject** [`\HomeCenter.Core\ComponentModel`]. This object have following properties
	- Uid  [`string`] - unique identifier for object
	- Type [`string`] - type of the object (for example type of **Command** or **Event**)
	- Tags [`List<string>`] - list of tags describing object
	- Properties [`Dictionary<string, Property>`] - key based dictionary of properties describing object
		- **Property** is general type that have type [`string`] and value od [`IValue`]
	- Properties can be manipulate by
		- `Maybe<IValue> GetPropertyValue(string propertyName, IValue defaultValue = null)` - return IValue
			- Maybe<IValue> - is special type from thin library https://github.com/vkhorikov/CSharpFunctionalExtensions and clarification fo using it can be found here https://www.pluralsight.com/courses/csharp-applying-functional-principles. It allows for Maybe type (this types shows that result can be null like nullable for value types) and ValueObject (add basic functionality like equality implementations)
		- `void SetPropertyValue(string propertyName, IValue value)` - set property value
			- IValue is implemented in [`HomeCenter.Core\ComponentModel\ValueTypes`] and have implementation for all basi property types like int [`IntValue`], double, string, bool datetime and string list. All inherit from ValueObject mention before for basic value object capabilities.
	- `this[string propertyName]` - indexer for easy access
	- Events use `Subject<Event>` from Reactive Extensions allowing for subscription for property change instead of standard delegates/events
- **Command** [`HomeCenter.Core\ComponentModel\Commands`] inherits from **BaseObject** and is used for sending command to Components/Adapters. Type property distinct command we are dealing with - known command types are defined in **CommandType** class. All parameters of command are in Properties (from BaseObject) - known command properties are defined in CommandProperties. Command also have CancelationToken for allowing cancel of long run operation.
- **Event** [`HomeCenter.Core\ComponentModel\Events`] inherits from **BaseObject** and is used for describing event in Component/Adapter. Special kind of Event - **PropertyChangedEvent** simplify creating property change events. Well known event types are defined in **EventType** and well known properties of event in **EventProperties**. Event sets SupressPropertyChangeEvent of base object to prevent circular references.
- **Actor** [`\HomeCenter.Core\ComponentModel\Components`] represents concept of actor model programming (https://en.wikipedia.org/wiki/Actor_model). It is base object for objects that allow interactions (Adapter, Component, maybe service in future). It allows for different style of programming where there is no locks and resource race. Every command send to actor go to special kind of queue (like message box) - in this implementation **BufferBlock** from TPL Dataflow library (https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) is used for this purpose. `Task<T> ExecuteCommand(Command command)`get Command as input and return Task that can be ignored or awaited if there is some result or we want to check if operation end without exception. Only ONE command is executed on the same time so we don't have to lock. This implementation gives safe invocation (it could be invoked by many threads at same time) and is compatible with async programming allowing for await for result. In addiction for easy of writing command handlers there is no need for writing mapping for Command type and handler - Actor implementation allows to write by convention so every method that get Command input and have name started with command type that end with Handler name are mapped for handling specific command - for example **TurnOn***Handler* is handling TurnOn command type.
	- **Actor** is also implementing **IService** so it have `Task Initialize()` and `Dispose` for cleaning all resources

	- To manage resources we want co to clean in `Dispose` we use special class **DisposeContainer** that holds references to all resources and we can just call Dispose on this class to free all resources

- **Component** [`\HomeCenter.Core\ComponentModel\Components`] inherits from **Actor** so it all capabilities described before. In addition component have

	- References to all adapters he is managing in `IList<AdapterReference>`- in `Initialize` method he sends **DiscoveryResponse** command to all adapters to query for capabilities of each Adapter. In response he gets list off all available **State** adapter can have and additionally all **required properties** that have to be send when communicating with adapter - for example HSREL8Adapter could handle turn on and turn off but we have to send pin number so adapter will know what pin we are working on.

		- **State** [HomeCenter.Core\ComponentModel\Capabilities] inherits from **BaseObject** and have default property `TimeOfValue` that have timestamp of state and additionally have `bool IsCommandSupported(Command command)`that allow for checking if `Command` is allowed to control this state. All well known states that adapter is exposing are defined in [HomeCenter.Core\ComponentModel\Capabilities] - for example **PowerState**, **VolumeState** etc. All have values written in **BaseOject**  `Properties` and this include `StateName` for the name of the state, `CapabilityName` (from **Constants.Capabilities** - I'm using capability not module because of Alexa naming conventions and all capabilities name are from Alexa docs - https://developer.amazon.com/docs/device-apis/alexa-powercontroller.html for example), `Value` for state value, `ValueList`for all available states the state can be, `SupportedCommands` list of all command types that can affect on state.

	- **Capabilities** [`Dictionary<string, State> _capabilities { get; }`] - current state of all capabilities from adapter

	- **Triggers** [`IList<Trigger> _triggers`] - allows to define mapping between **Event** that is raised by other components and subscribed in **EventAgregator** and corresponding **Command** that should be invoked when **Event** is detected - this is handled in **DeviceTriggerHandler** method. Triggers are read from json config file at start time. For example when we have component that is controlling a lamp we can subscribe for Event in button component.

	- **Converters** [`Dictionary<string, IValueConverter> _converters`] - allow to define converter for state value. For example when we have values from adapter temp sensor in kelvin and we want to display it in Celsius we can add converter for this

	- Command handling - all actions to components have to be executed with commands. Components have it own command like **SupportedCapabilitiesCommand** for list of supported capabilities, **SupportedStatesCommand** for supported states, **SupportedTagsCommand** that aggregate component all adapters tags, **GetStateCommand** to get value of specific state. All standard commands like TurnOn is forwarded to adapters (when adapter have **required properties** they are added to command before send)

	- Component have property **IsEnabled** that shows if component is turned on (is handling commands)

- **Adapter** inherits from Actor so it can have all behaviors from it. Additionally it has helper methods for update state and publish state change event [`Task<T> UpdateState<T>(string stateName, T oldValue, T newValue) where T : IValue`] and method for scheduling some periodic job using Quartz [`Task ScheduleDeviceRefresh<T>(TimeSpan interval) where T : IJob`]
	
	- Adapters have **IAdapterServiceFactory** in constructor for getting all basic services (I2c, quartz, event aggregator, logging services)

	- Adapter have **RequierdProperties** that inform component in **DiscoveryResponse** command that all command send to adapter have to fill those properties

	- All configuration of **Adapter** is read from config in **Properties** (form **BaseObject**) - additional configuration that is used per component (like pin number used by component of HSREL8Adapter ) is read from **AdapterRef** in component configuration.

	- Main functionality od **Adapter** is handling Commands and send PropertyChangeEvent from UpdateSatet method.
	- Current implementation of my adapters use messages like HttpMessage, TcpMessage (`\HomeCenter.Core\ComponentModel\Messages`) that are send by adapters using EventAggregator to corresponding protocol service like HttpMessagingService (`\HomeCenter.Core\Services\Networking`). It simplify adapter definiton to handle commands and compose message.


- **Area** - area configuration is read from config. Currently it is tree structure so each Area can have child Areas. Each area can have collection of components that are placed in area - in configuration component definition is put in separate place and are have only reference to component so it is easier to manage component placement.

- **Configuration** of Adapters, Components and Aras are stored in json file. Sample config is placed in test folder [`HomeCenter.Core.Tests\ComponentModel\SampleConfigs\componentConiguration.json`]. Configuration is deserialized in **ConfigurationService** to coresponding DTO objects [`HomeCenter.Core\ComponentModel\Configuration`] and then they are mapped to final objects using AutoMapper (http://automapper.org/). Automapper allows to map even private fields so we don't need to expose them publicly (to show which private field should be mapped there is additional MapAttribute). Configuration for mapping can be found in `\HomeCenter.Core\Services\DI\HomeCenterMappingProfile.cs`

- **DI** - project container for dependency injection is placed in `\HomeCenter.Core\Services\DI\HomeCenterContainer.cs`. In addition of usual stuff it contains `Queue<IService> GetSerives()`that return all services implementing IService and should be initialize by Initialize method (like in `StartupServices` extension of container `[\HomeCenter.Core\Extensions\ContainerExtensions.cs"]`

- Services - all services are in `\HomeCenter.Core\Services` where can find network services, quartz, logging and DI

- **EventAggregator** - is used for publish/subscribe event aggregator and it allows

	- Async work with async/await

	- Subscribe with message filter - simple key filter and key/value filter

	- Publish without response or Query for some result (in async way)

	- It has BehaviorChain functionality that allow to chain some behaviors like Timeout, retry or invoke in separate Task

	- We can subscribe for messages without EventAggregator reference when a type is created by DI container. All we have to do is to implement `IHandler<T>`where T is a message we are waiting. If we want to add additional filtering we can add `RoutingFilterAttribute` to method handler.

	- We can subscribe for messages using method handler or using `Observe<T>` to get IObservable (Reastive Extensions)

	- Test for EvenAggregator that shows menu use cases can be found here `HomeCenter.Core.Tests\EventAggregatorTests.cs`

- **Utils** - for now there is **AssemblyHelper** for getting all assemblies in solution

# Application flow

Flow of the application is following: When start all components, areas and adapters are read from config and initialized, additionally all services are initialized. When init components they read capabilities from adapters and can handle commands. Adapters reads initial states and run some pull jobs to query state in intervals (if we can read the state - there are some devices that can be controlled but we cannot read current state like my Samsung TV). Some basic setup is done in tests (`\HomeCenter.Core.Tests\ComponentModel`).
