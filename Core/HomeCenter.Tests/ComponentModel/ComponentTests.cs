using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Components;
using HomeCenter.Core.Tests.ComponentModel;
using HomeCenter.Messaging;
using HomeCenter.ComponentModel.Events;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Model.Extensions;

namespace HomeCenter.Extensions.Tests
{
    [TestClass]
    public class ComponentTests : ReactiveTest
    {
        [TestMethod]
        public async Task ComponentCommandExecuteShouldGetResult()
        {
            var dic = new Dictionary<string, string>();

            var (controller, container) = await new ControllerBuilder().WithConfiguration("componentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);


            await Task.Delay(TimeSpan.FromSeconds(1));


            var ev = new Event
            {
                Type = EventType.PropertyChanged
            };
            ev.SetPropertyValue(EventProperties.SourceDeviceUid, (StringValue)"HSPE16InputOnly_1");
            ev.SetPropertyValue("PinNumber", (IntValue)2);
            ev.SetPropertyValue(EventProperties.EventType, (StringValue)EventType.PropertyChanged);

            var eventAggregator = container.GetInstance<IEventAggregator>();
            await eventAggregator.PublishDeviceEvent(ev).ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromHours(5));

            var component = await controller.ExecuteCommand<Component>(CommandFatory.GetComponentCommand("RemoteLamp")).ConfigureAwait(false);


            var result = await component.ExecuteCommand<IEnumerable<string>>(CommandFatory.SupportedCapabilitiesCommand).ConfigureAwait(false);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("PowerState", result.FirstOrDefault());
        }
    }
}