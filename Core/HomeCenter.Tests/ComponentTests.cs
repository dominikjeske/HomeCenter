using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages.Events.Device;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    [TestClass]
    public class ComponentTests
    {
        [TestMethod]
        public async Task Component_EventTransform()
        {
            var result = await new ControllerBuilder().WithConfiguration("unitTestsSimpleScenario.json")
                                                      .BuildAndRun()
                                                      .ConfigureAwait(false);

            var me = result.broker.SubscribeAndWait<MotionEvent>();
            await result.adapter.PropertyChanged(PowerState.StateName, false, true);
            var ev = await me;

        }
    }
}