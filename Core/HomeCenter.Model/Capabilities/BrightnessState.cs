﻿using HomeCenter.Model.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class BrightnessState : StateBase
    {
        public BrightnessState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            //StateName = nameof(BrightnessState);
            //Capability = Constants.Capabilities.BrightnessController;

            this[StateProperties.MaxValue] = "100.0";
            this[StateProperties.MinValue] = "0.0";
            this[StateProperties.ValueUnit] = ValueUnit.Percent;
        }
    }
}