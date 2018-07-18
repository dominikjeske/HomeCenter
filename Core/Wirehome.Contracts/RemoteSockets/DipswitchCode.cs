using System;

namespace Wirehome.Core.Hardware.RemoteSockets
{
    public class DipswitchCode : IEquatable<DipswitchCode>
    {
        public DipswitchSystemCode System { get; }
        public DipswitchUnitCode Unit { get; }
        public RemoteSocketCommand Command { get; }
        private uint _code;

        public DipswitchCode(DipswitchSystemCode system, DipswitchUnitCode unit, RemoteSocketCommand command)
        {
            System = system;
            Unit = unit;
            Command = command;

            _code = GetCode(this);
        }

        public uint Code => _code;

        public static DipswitchCode ParseCode(string system, string unit, string command) =>
                     new DipswitchCode((DipswitchSystemCode)Enum.Parse(typeof(DipswitchSystemCode), system), 
                                       (DipswitchUnitCode)Enum.Parse(typeof(DipswitchUnitCode), unit), 
                                       (RemoteSocketCommand)Enum.Parse(typeof(RemoteSocketCommand), command)
                                      );
        

        public static DipswitchCode ParseCode(uint code)
        {
            var command = ParseCommand(code);
            if (!command.HasValue) return null;

            var unit = ParseUnit(code);
            if (!unit.HasValue) return null;

            var system = ParseSystem(code);

            return new DipswitchCode(system, unit.GetValueOrDefault(), command.GetValueOrDefault());
        }

        public static uint GetCode(DipswitchCode code)
        {
            uint calc = 0U;
            calc = SetSystemCode(calc, code.System);
            calc = SetUnitCode(calc, code.Unit);
            calc = SetCommand(calc, code.Command);
            return calc;
        }

        public string ToShortCode() => $"{System.ToString()}|{Unit.ToString()}";

        private static RemoteSocketCommand? ParseCommand(uint code)
        {
            var commandMask = "00000000 00000000 00000000 00111111";

            commandMask = commandMask.Replace(" ", "");

            var commandMaskValue = Convert.ToInt32(commandMask, 2);

            var maskedCommand = code & commandMaskValue;

            if (maskedCommand == 0x11)
            {
                return RemoteSocketCommand.TurnOn;
            }
            else if (maskedCommand == 0x14)
            {
                return RemoteSocketCommand.TurnOff;
            }

            return null;
        }

        private static DipswitchUnitCode? ParseUnit(uint code)
        {
            var unitMask = "00000000 00000000 00011111 11000000";

            unitMask = unitMask.Replace(" ", "");

            var unitMaskValue = Convert.ToInt32(unitMask, 2);

            var maskedUnit = code & unitMaskValue;

            maskedUnit = maskedUnit >> 6;

            if (maskedUnit == 0x15)
            {
                return DipswitchUnitCode.A;
            }
            else if (maskedUnit == 0x45)
            {
                return DipswitchUnitCode.B;
            }
            else if (maskedUnit == 0x51)
            {
                return DipswitchUnitCode.C;
            }
            else if (maskedUnit == 0x54)
            {
                return DipswitchUnitCode.D;
            }

            return null;
        }

        private static DipswitchSystemCode ParseSystem(uint code)
        {
            DipswitchSystemCode parsedCode = DipswitchSystemCode.AllOff;

            var systemMask = "00000000 00000000 00011000 00000000";

            systemMask = systemMask.Replace(" ", "");

            var systemMaskValue = Convert.ToInt32(systemMask, 2);

            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch1);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch2);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch3);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch4);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch5);

            return parsedCode;
        }

        private static int CheckNextSystem(ref DipswitchSystemCode parsedCode, int mask, uint code, DipswitchSystemCode systemCode)
        {
            mask = mask << 2;
            var maskedSysten = code & mask;

            if (maskedSysten == 0)
            {
                parsedCode |= systemCode;
            }

            return mask;
        }

        
        private static uint SetSystemCode(uint code, DipswitchSystemCode systemCode)
        {
            // A LOW switch is binary 10 and a HIGH switch is binary 00.
            // The values of the DIP switches are inverted.
            if (!systemCode.HasFlag(DipswitchSystemCode.Switch1))
            {
                code |= 1U << 22;
            }

            if (!systemCode.HasFlag(DipswitchSystemCode.Switch2))
            {
                code |= 1U << 20;
            }

            if (!systemCode.HasFlag(DipswitchSystemCode.Switch3))
            {
                code |= 1U << 18;
            }

            if (!systemCode.HasFlag(DipswitchSystemCode.Switch4))
            {
                code |= 1U << 16;
            }

            if (!systemCode.HasFlag(DipswitchSystemCode.Switch5))
            {
                code |= 1U << 14;
            }

            return code;
        }

        private static uint SetUnitCode(uint code, DipswitchUnitCode unitCode)
        {
            uint unitCodeValue;

            switch (unitCode)
            {
                case DipswitchUnitCode.A:
                    {
                        unitCodeValue = 0x15;
                        break;
                    }

                case DipswitchUnitCode.B:
                    {
                        unitCodeValue = 0x45;
                        break;
                    }

                case DipswitchUnitCode.C:
                    {
                        unitCodeValue = 0x51;
                        break;
                    }

                case DipswitchUnitCode.D:
                    {
                        unitCodeValue = 0x54;
                        break;
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }

            code |= unitCodeValue << 6;
            return code;
        }

        private static uint SetCommand(uint code, RemoteSocketCommand command)
        {
            switch (command)
            {
                case RemoteSocketCommand.TurnOn:
                    {
                        code |= 0x11;
                        break;
                    }

                case RemoteSocketCommand.TurnOff:
                    {
                        code |= 0x14;
                        break;
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }

            return code;
        }



        public bool Equals(DipswitchCode other)
        {
            return AreEqual(this, other);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DipswitchCode;

            return AreEqual(this, other);
        }

        public override int GetHashCode()
        {
            return System.GetHashCode() ^ Unit.GetHashCode() ^ Command.GetHashCode();
        }

        public bool AreEqual(DipswitchCode a, DipswitchCode b)
        {
            if (a.System == b.System && a.Command == b.Command && a.Unit == b.Unit)
                return true;

            return false;
        }
    }

    // Examples:
    // System Code = 11111
    //00000000|00000000000|0010101|010001 = 1361 A ON
    //00000000|00000000000|0010101|010100 = 1364 A OFF
    //00000000|00000000000|1000101|010001 = 4433 B ON
    //00000000|00000000000|1000101|010100 = 4436 B OFF
    //00000000|00000000000|1010001|010001 = 5201 C ON
    //00000000|00000000000|1010001|010100 = 5204 C OFF
    //00000000|00000000000|1010100|010001 = 5393 D ON
    //00000000|00000000000|1010100|010100 = 5396 D OFF
    // System Code = 00000
    //00000000|01010101010|0010101|010001 = 5588305 A ON
    //00000000|01010101010|0010101|010100 = 5588308 A OFF
    //00000000|01010101010|1000101|010001 = 5591377 B ON
    //00000000|01010101010|1000101|010100 = 5591380 B OFF
    //00000000|01010101010|1010001|010001 = 5592145 C ON
    //00000000|01010101010|1010001|010100 = 5592148 C OFF
    //00000000|01010101010|1010100|010001 = 5592337 D ON
    //00000000|01010101010|1010100|010100 = 5592340 D OFF
    // System Code = 10101
    //00000000|00010001000|0010101|010001 = 1115473 A ON
    //00000000|00010001000|0010101|010100 = 1115476 A OFF
    //00000000|00010001000|1000101|010001 = 1118545 B ON
    //00000000|00010001000|1000101|010100 = 1118548 B OFF
    //00000000|00010001000|1010001|010001 = 1119313 C ON
    //00000000|00010001000|1010001|010100 = 1119316 C OFF
    //00000000|00010001000|1010100|010001 = 1119505 D ON
    //00000000|00010001000|1010100|010100 = 1119508 D OFF

}