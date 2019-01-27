
//[TestMethod]
//public async Task ManualLightChangeShouldInvokeDecoderAction()
//{
//    var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithSampleDecoder(() => new IEventDecoder[] { new DisableAutomationDecoder() })
//                                                                                                .WithLampEvents
//                                                                                                (
//                                                                                                    OnNext(Time.Tics(500), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
//                                                                                                    OnNext(Time.Tics(1500), PowerStateChangeEvent.Create(false, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
//                                                                                                    OnNext(Time.Tics(2000), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource))
//                                                                                                )
//                                                                                                .Build();

//    scheduler.AdvanceTo(service.Configuration.ManualCodeWindow.Add(TimeSpan.FromMilliseconds(500)));

//    Assert.AreEqual(true, await _context.Query<bool>(AutomationStateQuery.Create(Detectors.kitchenDetector)));
//}

//    [TestMethod]
//    public void ManualLightChangeShouldNotInvokeDecoderActionWhenIncomplete()
//    {
//        var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithSampleDecoder(() => new IEventDecoder[] { new DisableAutomationDecoder() })
//                                                                                                    .WithLampEvents
//                                                                                                    (
//                                                                                                        OnNext(Time.Tics(500), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
//                                                                                                        OnNext(Time.Tics(1500), PowerStateChangeEvent.Create(false, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource))
//                                                                                                    )
//                                                                                                    .Build();

//        service.Start();
//        scheduler.AdvanceTo(service.Configuration.ManualCodeWindow.Add(TimeSpan.FromMilliseconds(500)));

//        Assert.AreEqual(false, service.IsAutomationDisabled(Detectors.kitchenDetector));
//    }

//    [TestMethod]
//    public void ManualLightChangeShouldNotInvokeDecoderActionWhenToLong()
//    {
//        var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithSampleDecoder(() => new IEventDecoder[] { new DisableAutomationDecoder() })
//                                                                                                    .WithLampEvents
//                                                                                                    (
//                                                                                                        OnNext(Time.Tics(500), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
//                                                                                                        OnNext(Time.Tics(1500), PowerStateChangeEvent.Create(false, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
//                                                                                                        OnNext(Time.Tics(4000), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource))
//                                                                                                    )
//                                                                                                    .Build();

//        service.Start();
//        scheduler.AdvanceTo(service.Configuration.ManualCodeWindow.Add(TimeSpan.FromMilliseconds(500)));

//        Assert.AreEqual(false, service.IsAutomationDisabled(Detectors.kitchenDetector));
//    }
