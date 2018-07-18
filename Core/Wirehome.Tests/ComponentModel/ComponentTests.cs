﻿using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Components;
using HomeCenter.Core.Tests.ComponentModel;

namespace HomeCenter.Extensions.Tests
{
    [TestClass]
    public class ComponentTests : ReactiveTest
    {
        [TestMethod]
        public async Task ComponentCommandExecuteShouldGetResult()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("componentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);


            await Task.Delay(TimeSpan.FromHours(1));

            var component = await controller.ExecuteCommand<Component>(CommandFatory.GetComponentCommand("RemoteLamp")).ConfigureAwait(false);


            var result = await component.ExecuteCommand<IEnumerable<string>>(CommandFatory.SupportedCapabilitiesCommand).ConfigureAwait(false);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("PowerState", result.FirstOrDefault());
        }
    }
}