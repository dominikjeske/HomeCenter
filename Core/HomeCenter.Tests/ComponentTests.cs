using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Tests.Dummies;
using HomeCenter.Tests.Helpers;
using HomeCenter.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    [TestClass]
    public class ComponentTests
    {
        private static TestAdapter _adapter;
        private static RequiredPropertiesAdapter _adapterWithRequiredProps;
        private static IMessageBroker _messageBroker;
        private static LogMock _logs;

        [ClassInitialize]
        public static async Task Init(TestContext context)
        {
            var result = await new ControllerBuilder().WithConfiguration("unitTestsSimpleScenario.json").BuildAndRun().ConfigureAwait(false);
            _messageBroker = result.broker;
            _logs = result.logs;
            _adapter = await _messageBroker.Request<GetAdapterQuery, TestAdapter>(new GetAdapterQuery(), "TestAdapter").ConfigureAwait(false);
            _adapterWithRequiredProps = await _messageBroker.Request<GetAdapterQuery, RequiredPropertiesAdapter>(new GetAdapterQuery(), "RequiredPropertiesAdapter").ConfigureAwait(false);
        }

        [TestInitialize]
        public void TestInit()
        {
            _logs.Clear();
        }

        [TestMethod]
        public async Task Component_PropertyChangeEventTransform()
        {
            var motionEvent = await TaskHelper.Execute
            (
                () => _messageBroker.SubscribeAsync<MotionEvent>(),
                async () => await _adapter.PropertyChanged(PowerState.StateName, false, true)
            );

            Assert.AreEqual(typeof(MotionEvent), motionEvent.GetType());
            Assert.AreEqual(nameof(MotionEvent), motionEvent.Type);
            Assert.AreEqual("TestAdapter", motionEvent.MessageSource);
            Assert.IsFalse(_logs.Messages.Any(x => x.LogLevel == LogLevel.Error));
        }

        [TestMethod]
        public async Task Component_CommandTransform()
        {
            var command = await TaskHelper.Execute
            (
                () => _adapter.CommandRecieved.FirstAsync().ToTask(),
                () => _messageBroker.Send(TurnOnCommand.Default, "TestComponent")
            );

            Assert.AreEqual(typeof(TurnOnCommand), command.GetType());
            Assert.IsTrue(command.ContainsProperty("StateTime"));
            Assert.AreEqual(command.AsInt("StateTime"), 200);
            Assert.IsFalse(_logs.Messages.Any(x => x.LogLevel == LogLevel.Error));
        }

        [TestMethod]
        public async Task Component_UnsupportedCommand()
        {
            var logentry = await TaskHelper.Execute
            (
                () => _logs.MessageSink.Where(m => m.LogLevel == LogLevel.Error).FirstAsync().ToTask(),
                () => _messageBroker.Send(VolumeDownCommand.Default, "TestComponent"), 1000
            );

            Assert.IsTrue(logentry.Message.IndexOf("cannot process message because there is no registered handler for [VolumeDownCommand])") > -1);
        }

        [TestMethod]
        public async Task Component_ShouldAdd_RequiredProperties()
        {
            var command = await TaskHelper.Execute
            (
                () => _adapterWithRequiredProps.CommandRecieved.FirstAsync().ToTask(),
                () => _messageBroker.Send(TurnOnCommand.Default, "RequiredPropertiesComponent")
            );

            Assert.IsFalse(_logs.Messages.Any(x => x.LogLevel == LogLevel.Error));
            Assert.IsTrue(command.ContainsProperty(MessageProperties.PinNumber));                          // Component should add this property
        }

        [TestMethod]
        public async Task Component_PropertyChangeEvent_ShouldHaveRequiredProp()
        {
            var ev = await TaskHelper.Execute
            (
                () => _messageBroker.SubscribeAsync<PropertyChangedEvent>(),
                async () => await _adapterWithRequiredProps.PropertyChanged(PowerState.StateName, false, true)
            );

            Assert.AreEqual(typeof(PropertyChangedEvent), ev.GetType());
            Assert.AreEqual(nameof(PropertyChangedEvent), ev.Type);
            Assert.AreEqual("RequiredPropertiesAdapter", ev.MessageSource);
            Assert.IsFalse(_logs.Messages.Any(x => x.LogLevel == LogLevel.Error));
            Assert.IsTrue(ev.ContainsProperty(MessageProperties.PinNumber));
        }
    }
}