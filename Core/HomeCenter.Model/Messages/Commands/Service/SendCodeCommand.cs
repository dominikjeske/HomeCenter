namespace HomeCenter.Model.Messages.Commands.Device
{
    public class SendCodeCommand : Command
    {
        public uint Code
        {
            get => AsUint(nameof(Code));
            set => SetProperty(nameof(Code), value);
        }

        public int System
        {
            get => AsInt(nameof(System));
            set => SetProperty(nameof(System), value);
        }

        public int Bits
        {
            get => AsInt(nameof(Bits));
            set => SetProperty(nameof(Bits), value);
        }

        public int Repeat
        {
            get => AsInt(nameof(Repeat));
            set => SetProperty(nameof(Repeat), value);
        }

        public static SendCodeCommand Create(uint code, int system = 7, int bits = 32, int repeat = 1) => new SendCodeCommand { Code = code, System = system, Bits = bits, Repeat = repeat };
    }
}