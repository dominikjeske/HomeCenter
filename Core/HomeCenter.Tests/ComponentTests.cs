using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Services.Controllers;
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
        private (IMessageBroker broker, LogMock logs, ITestAdapter adapter) _builder;
        private IMessageBroker _broker => _builder.broker;
        private ITestAdapter _adapter => _builder.adapter;
        private LogMock _logs => _builder.logs;

        [TestCleanup]
        public async Task CleanUp()
        {
             await _broker.Request<StopSystemQuery, bool>(StopSystemQuery.Default, nameof(Controller)).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task Component_PropertyChangeEventTransform()
        {
            _builder = await new ControllerBuilder().WithConfiguration("unitTestsSimpleScenario.json").WithAdapter("SimpleAdapter").BuildAndRun().ConfigureAwait(false);

            var motionEvent = await TaskHelper.Execute
            (
                () => _broker.SubscribeAsync<MotionEvent>(),
                async () => await _adapter.PropertyChanged(PowerState.StateName, false, true)
            );

            Assert.AreEqual(typeof(MotionEvent), motionEvent.GetType());
            Assert.AreEqual(nameof(MotionEvent), motionEvent.Type);
            Assert.AreEqual(_adapter.Uid, motionEvent.MessageSource);
            Assert.IsFalse(_logs.HasErrors);
        }

        [TestMethod]
        public async Task Component_CommandTransform()
        {
            _builder = await new ControllerBuilder().WithConfiguration("unitTestsSimpleScenario.json").WithAdapter("SimpleAdapter").BuildAndRun().ConfigureAwait(false);

            var command = await TaskHelper.Execute
            (
                () => _adapter.CommandRecieved.FirstAsync().ToTask(),
                () => _broker.Send(TurnOnCommand.Default, "TestComponent")
            );

            Assert.AreEqual(typeof(TurnOnCommand), command.GetType());
            Assert.IsTrue(command.ContainsProperty("StateTime"));
            Assert.AreEqual(command.AsInt("StateTime"), 200);
            Assert.IsFalse(_logs.HasErrors);
        }

        [TestMethod]
        public async Task Component_UnsupportedCommand()
        {
            _builder = await new ControllerBuilder().WithConfiguration("unitTestsSimpleScenario.json").WithAdapter("SimpleAdapter").BuildAndRun().ConfigureAwait(false);

            var logentry = await TaskHelper.Execute
            (
                () => _logs.MessageSink.Where(m => m.LogLevel == LogLevel.Error).FirstAsync().ToTask(),
                () => _broker.Send(VolumeDownCommand.Default, "TestComponent"), 1000
            );

            Assert.IsTrue(logentry.Message.IndexOf("cannot process message because there is no registered handler for [VolumeDownCommand]") > -1);
        }

        [TestMethod]
        public async Task Component_ShouldAdd_RequiredProperties()
        {
            _builder = await new ControllerBuilder().WithConfiguration("unitTestsSimpleScenario.json").WithAdapter("RequiredPropertiesAdapter").BuildAndRun().ConfigureAwait(false);

            var command = await TaskHelper.Execute
            (
                () => _adapter.CommandRecieved.FirstAsync().ToTask(),
                () => _broker.Send(TurnOnCommand.Default, "RequiredPropertiesComponent")
            );

            Assert.IsTrue(command.ContainsProperty(MessageProperties.PinNumber));                          // Component should add this property
            Assert.IsFalse(_logs.HasErrors);
        }

        [TestMethod]
        public async Task Component_PropertyChangeEvent_ShouldHaveRequiredProp()
        {
            _builder = await new ControllerBuilder().WithConfiguration("unitTestsSimpleScenario.json").WithAdapter("RequiredPropertiesAdapter").BuildAndRun().ConfigureAwait(false);

            var ev = await TaskHelper.Execute
            (
                () => _broker.SubscribeAsync<PropertyChangedEvent>(),
                async () => await _adapter.PropertyChanged(PowerState.StateName, false, true)
            );

            Assert.AreEqual(typeof(PropertyChangedEvent), ev.GetType());
            Assert.AreEqual(nameof(PropertyChangedEvent), ev.Type);
            Assert.AreEqual("RequiredPropertiesAdapter", ev.MessageSource);
            Assert.IsTrue(ev.ContainsProperty(MessageProperties.PinNumber));
            Assert.IsFalse(_logs.HasErrors);
        }
    }
}