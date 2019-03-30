using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;


namespace HomeCenter.Tests.ComponentModel
{
    [TestClass]
    public class ComponentTests : BaseTest
    {
        [TestMethod]
        public async Task Component_PropertyChangeEventTransform()
        {
            var controller = await new ControllerBuilder(Container).WithConfiguration("TranslateAdapter.json").BuildAndRun();
            var adapter = await GetAdapter<SimpleAdapter>();

            var motionEvent = await Broker.WaitForEvent<MotionEvent>(async () => await adapter.PropertyChanged(PowerState.StateName, false, true));

            Assert.IsFalse(Logs.HasErrors);
            Assert.AreEqual(typeof(MotionEvent), motionEvent.GetType());
            Assert.AreEqual(nameof(MotionEvent), motionEvent.Type);
            Assert.AreEqual(adapter.Uid, motionEvent.MessageSource);
        }

        [TestMethod]
        public async Task Component_CommandTransform()
        {
            var controller = await new ControllerBuilder(Container).WithConfiguration("TranslateAdapter.json").BuildAndRun();
            var adapter = await GetAdapter<SimpleAdapter>();

            var command = await adapter.CommandRecieved
                                       .ToFirstTask()
                                       .WaitForMessage(() => Broker.Send(TurnOnCommand.Default, "TestComponent"));

            Assert.AreEqual(typeof(TurnOnCommand), command.GetType());
            Assert.IsTrue(command.ContainsProperty("StateTime"));
            Assert.AreEqual(command.AsInt("StateTime"), 200);
            Assert.IsFalse(Logs.HasErrors);
        }

        [TestMethod]
        public async Task Component_UnsupportedCommand()
        {
            var controller = await new ControllerBuilder(Container).WithConfiguration("SimpleAdapter.json").BuildAndRun();
            var adapter = await GetAdapter<SimpleAdapter>();

            var logentry = await Logs.MessageSink
                                     .Where(m => m.LogLevel == LogLevel.Error)
                                     .ToFirstTask()
                                     .WaitForMessage(() => Broker.Send(VolumeDownCommand.Default, "TestComponent"));

            Assert.IsTrue(logentry.Message.IndexOf("cannot process message because there is no registered handler for [VolumeDownCommand]") > -1);
        }

        [TestMethod]
        public async Task Component_ShouldAdd_RequiredProperties()
        {
            var controller = await new ControllerBuilder(Container).WithConfiguration("RequiredProperties.json").BuildAndRun();
            var adapter = await GetAdapter<RequiredPropertiesAdapter>();

            var command = await adapter.CommandRecieved
                                       .ToFirstTask()
                                       .WaitForMessage(() => Broker.Send(TurnOnCommand.Default, "RequiredPropertiesComponent"));

            Assert.IsFalse(Logs.HasErrors);
            Assert.IsTrue(command.ContainsProperty(MessageProperties.PinNumber));                          // Component should add this property
        }

        [TestMethod]
        public async Task Component_PropertyChangeEvent_ShouldHaveRequiredProp()
        {
            var controller = await new ControllerBuilder(Container).WithConfiguration("RequiredProperties.json").BuildAndRun();
            var adapter = await GetAdapter<RequiredPropertiesAdapter>();

            var messages = await Broker.WaitForEvents<PropertyChangedEvent>(async () => await adapter.PropertyChanged(PowerState.StateName, false, true), "*");

            var ev = messages.Where(x => x.MessageSource == "RequiredPropertiesAdapter").Single();
            Assert.IsFalse(Logs.HasErrors);
            Assert.AreEqual(typeof(PropertyChangedEvent), ev.GetType());
            Assert.AreEqual(nameof(PropertyChangedEvent), ev.Type);
            Assert.IsTrue(ev.ContainsProperty(MessageProperties.PinNumber));
        }


        [TestMethod]
        public async Task Component_PropertyChangeEvent_SharedAdapter()
        {
            var controller = await new ControllerBuilder(Container).WithConfiguration("SharedAdapter.json").BuildAndRun();
            var adapter = await GetAdapter<RequiredPropertiesAdapter>();

            var messages = await Broker.WaitForEvents<PropertyChangedEvent>(async () => await adapter.PropertyChanged(PowerState.StateName, false, true, 1), "*", 1000);

            var messageFromC1 = messages.Where(x => x.MessageSource == "C1").Single();
            var messageFromC2 = messages.Where(x => x.MessageSource == "C2").FirstOrDefault();
            Assert.IsFalse(Logs.HasErrors);
            Assert.IsNotNull(messageFromC1);
            Assert.IsNull(messageFromC2);
        }
    }
}