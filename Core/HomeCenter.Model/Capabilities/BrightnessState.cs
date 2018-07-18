using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.ValueTypes;

namespace HomeCenter.ComponentModel.Capabilities
{
    public class BrightnessState : State
    {
        public BrightnessState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            //StateName = nameof(BrightnessState);
            //Capability = Constants.Capabilities.BrightnessController;

            this[StateProperties.Value] = new DoubleValue();
            this[StateProperties.MaxValue] = new DoubleValue(100.0);
            this[StateProperties.MinValue] = new DoubleValue(0.0);
            this[StateProperties.ValueUnit] = (StringValue)ValueUnit.Percent;
        }
    }
}
