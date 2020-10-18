using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Components;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Tests.ComponentModel
{
    [TestClass]
    public class ComponentStateTests
    {
        [TestMethod]
        public void GetCommandAdapter_WhenOneWriteAndTwoRead_ShouldReturnWriteAdapter()
        {
            var adapterResponse = GetTwoReadOneWriteAdapters();
            var state = new ComponentState(adapterResponse);

            var result = state.GetCommandAdapter(new TurnOnCommand());

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("WriteAdapter", result.First());
        }

        [TestMethod]
        public void GetCommandAdapter_WhenCommandNotSupported_ShouldReturnZeroElements()
        {
            var adapterResponse = GetTwoReadOneWriteAdapters();
            var state = new ComponentState(adapterResponse);

            var result = state.GetCommandAdapter(new VolumeDownCommand());

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetCommandAdapter_ReadWriteAdapter_ShouldReturnThisAdapter()
        {
            var adapterResponse = GetReadWriteAdapter();
            var state = new ComponentState(adapterResponse);

            var result = state.GetCommandAdapter(new TurnOnCommand());

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Adapter", result.First());
        }

        [TestMethod]
        public void IsStateProvidingAdapter_WhenTwoReadAndOneExplicit_ReturnTrueOnExplicit()
        {
            var adapterResponse = GetTwoReadOneWriteAdapters();
            var state = new ComponentState(adapterResponse);

            var result = state.IsStateProvidingAdapter("ReadAdapter", PowerState.StateName);
            
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsStateProvidingAdapter_WhenTwoReadAndOneExplicit_ReturnFalseOnNotExplicit()
        {
            var adapterResponse = GetTwoReadOneWriteAdapters();
            var state = new ComponentState(adapterResponse);

            var result = state.IsStateProvidingAdapter("ReadAdapter2", PowerState.StateName);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsStateProvidingAdapter_WhenTwoReadAndOneWrite_ReturnFalseOnWrite()
        {
            var adapterResponse = GetTwoReadOneWriteAdapters();
            var state = new ComponentState(adapterResponse);

            var result = state.IsStateProvidingAdapter("WriteAdapter", PowerState.StateName);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsStateProvidingAdapter_WhenStateNotExists_ShouldReturnFalse()
        {
            var adapterResponse = GetTwoReadOneWriteAdapters();
            var state = new ComponentState(adapterResponse);

            var result = state.IsStateProvidingAdapter("ReadAdapter", "MissingState");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsStateProvidingAdapter_WhenReadWriteAdapter_ShouldReturnTrue()
        {
            var adapterResponse = GetReadWriteAdapter();
            var state = new ComponentState(adapterResponse);

            var result = state.IsStateProvidingAdapter("Adapter", PowerState.StateName);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryUpdateState_ExecutedOnExistingState_ShouldUpdateState()
        {
            var adapterResponse = GetReadWriteAdapter();
            var state = new ComponentState(adapterResponse);

            var result = state.TryUpdateState(PowerState.StateName, "1", out var old);
            var value = state.GetStateValues(PowerState.StateName).FirstOrDefault().Value;
            
            Assert.IsTrue(result);
            Assert.IsNull(old);
            Assert.AreEqual("1", value);
        }

        [TestMethod]
        public void TryUpdateState_WhenExecutingTwoTimes_ShouldUpdateStateAndReturnPrevous()
        {
            var adapterResponse = GetReadWriteAdapter();
            var state = new ComponentState(adapterResponse);

            var result = state.TryUpdateState(PowerState.StateName, "1", out var old);
            result = state.TryUpdateState(PowerState.StateName, "0", out old);
            var value = state.GetStateValues(PowerState.StateName).FirstOrDefault().Value;

            Assert.IsTrue(result);
            Assert.AreEqual("1", old);
            Assert.AreEqual("0", value);
        }


        private static Dictionary<AdapterReference, DiscoveryResponse> GetTwoReadOneWriteAdapters()
        {
            return new Dictionary<AdapterReference, DiscoveryResponse>()
            {
                [(AdapterReference)(new AdapterReference() { Uid = "ReadAdapter" }).SetProperty(PowerState.StateName, ReadWriteMode.Read)] = new DiscoveryResponse(new PowerState(ReadWriteMode.Read)),
                [new AdapterReference() { Uid = "ReadAdapter2" }] = new DiscoveryResponse(new PowerState(ReadWriteMode.Read)),
                [new AdapterReference() { Uid = "WriteAdapter" }] = new DiscoveryResponse(new PowerState(ReadWriteMode.Write))
            };
        }

        private static Dictionary<AdapterReference, DiscoveryResponse> GetReadWriteAdapter()
        {
            return new Dictionary<AdapterReference, DiscoveryResponse>()
            {
                [new AdapterReference() { Uid = "Adapter" }] = new DiscoveryResponse(new PowerState())
            };
        }
    }
}