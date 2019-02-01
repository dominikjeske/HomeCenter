using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Tests.Helpers;
using HomeCenter.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    [TestClass]
    public class ComponentTests
    {
        private static TestAdapter _adapter;
        private static IMessageBroker _messageBroker;

        [ClassInitialize]
        public static async Task Init(TestContext context)
        {
            var result = await new ControllerBuilder().WithConfiguration("unitTestsSimpleScenario.json").BuildAndRun();
            _adapter = result.adapter;
            _messageBroker = result.broker;
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
        }
    }
}