using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Core.Tests.ComponentModel;
using HomeCenter.Messaging;
using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Extensions.Tests
{
    [TestClass]
    public class ComponentIntegrationTests : ReactiveTest
    {
        [TestMethod]
        public async Task TestReadComponentsFromConfig()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);
            var eventAggregator = container.GetInstance<IEventAggregator>();

            var properyChangeEvent = new PropertyChangedEvent("HSPE16InputOnly_1", PowerState.StateName, new BooleanValue(false),
                                     new BooleanValue(true), new Dictionary<string, IValue>() { { AdapterProperties.PinNumber, new IntValue(2) } });

            await eventAggregator.PublishDeviceEvent(properyChangeEvent, new string[] { AdapterProperties.PinNumber }).ConfigureAwait(false);

            //var lamp = configuration.Components.FirstOrDefault(c => c.Uid == "Lamp1");
            //await lamp.ExecuteCommand(new Command { Type = CommandType.TurnOn }).ConfigureAwait(false);

            //await Task.Delay(5000);
        }

        [TestMethod]
        public async Task TestRemoteLamp()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);
            var eventAggregator = container.GetInstance<IEventAggregator>();

            //TODO
            //var lamp = config.config.Components.FirstOrDefault(c => c.Uid == "RemoteLamp");
            //await lamp.ExecuteCommand(Command.TurnOnCommand).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestRun()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);
            var eventAggregator = container.GetInstance<IEventAggregator>();

            //  await controller.ExecuteQuery<DeviceSearchQuery>(DeviceSearchQuery.Query("uid"));

            //await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
            //TODO
            //var lamp = config.Components.FirstOrDefault(c => c.Uid == "RemoteLamp");
            //await lamp.ExecuteCommand(Command.TurnOnCommand).ConfigureAwait(false);
        }
    }
}