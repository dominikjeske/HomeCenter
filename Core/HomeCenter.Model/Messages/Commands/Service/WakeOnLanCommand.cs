using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace HomeCenter.Model.Messages.Commands.Service
{
   
    public class WakeOnLanCommand : UdpCommand
    {
        public WakeOnLanCommand(string macAddress)
        {
            Address = FormatAddress(macAddress);
            Body = FormatBody();
        }

        private string FormatAddress(string macAddress)
        {
            var MAC = Regex.Replace(macAddress, "[^0-9A-Fa-f]", "");

            if (MAC.Length != 12)
            {
                throw new ArgumentException("Invalid MAC address. Try again!");
            }

            //255.255.255.255  i.e broadcast, port = 12287
            var address = new IPAddress(0xffffffff);
            return $"{address}:{12287}";
        }

        private byte[] FormatBody()
        {
            int byteCount = 0;
            var bytes = new byte[102];
            for (int trailer = 0; trailer < 6; trailer++)
            {
                bytes[byteCount++] = 0xFF;
            }
            for (int macPackets = 0; macPackets < 16; macPackets++)
            {
                int i = 0;
                for (int macBytes = 0; macBytes < 6; macBytes++)
                {
                    bytes[byteCount++] =
                    byte.Parse(Address.Substring(i, 2), NumberStyles.HexNumber);
                    i += 2;
                }
            }
            return bytes;
        }
    }
}