using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Capabilities
{
    public class BrightnessState : StateBase
    {
        public BrightnessState([AllowNull] string ReadWriteMode = default) : base(ReadWriteMode)
        {
            //StateName = nameof(BrightnessState);
            //Capability = Constants.Capabilities.BrightnessController;

            this[StateProperties.MaxValue] = "100.0";
            this[StateProperties.MinValue] = "0.0";
            this[StateProperties.ValueUnit] = ValueUnit.Percent;
        }
    }
}