using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeCenter.Adapters.Samsung.Messages
{
    public class SamsungControlCommand : TcpCommand, IFormatableMessage<SamsungControlCommand>
    {
        public string Code { get; set; }
        public int Port { get; set; } = 55000;

        private readonly string _appKey = "HomeCenter";
        private readonly string _nullValue = char.ToString((char)0x00);
        private readonly string _appString = "samsung.remote";

        public SamsungControlCommand FormatMessage()
        {
            Address = $"{Address}:{Port}";
            Body = Serialize();

            return this;
        }

        private byte[] CreateIdentifier()
        {
            //TODO check is this correct?
            var myIpBase64 = Base64Encode("192.168.1.35");
            var myMacBase64 = Base64Encode("0C-89-10-CD-43-28");
            var nameBase64 = Base64Encode(_appKey);

            var message =
                char.ToString((char)0x64) +
                _nullValue +
                Format(myIpBase64.Length) +
                _nullValue +
                myIpBase64 +
                Format(myMacBase64.Length) +
                _nullValue +
                myMacBase64 +
                Format(nameBase64.Length) +
                _nullValue +
                nameBase64;
            var wrappedMessage = _nullValue + Format(_appString.Length) + _nullValue + _appString + Format(message.Length) + _nullValue + message;

            return ConvertToBytes(wrappedMessage);
        }

        private byte[] Serialize()
        {
            var identifier = CreateIdentifier();
            var secondParameter = CreateSecondParameter();
            var command = CreateCommand(Code);

            var binaryMessage = new List<byte>();
            binaryMessage.AddRange(identifier);
            binaryMessage.AddRange(secondParameter);
            binaryMessage.AddRange(command);

            return binaryMessage.ToArray();
        }
        
        private byte[] CreateSecondParameter()
        {
            var message = ((char)0xc8) + ((char)0x00) + string.Empty;

            var wrappedMessage = _nullValue + Format(_appString.Length) + _nullValue + _appString + Format(message.Length) + _nullValue + message;
            return ConvertToBytes(wrappedMessage);
        }

        private byte[] CreateCommand(string command)
        {
            var encodedCommand = Base64Encode(command);

            var message = _nullValue + _nullValue + _nullValue + char.ToString((char)encodedCommand.Length) + _nullValue + encodedCommand;
            var wrappedMessage = _nullValue + Format(_appString.Length) + _nullValue + _appString + Format(message.Length) + _nullValue + message;

            return ConvertToBytes(wrappedMessage);
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private byte[] ConvertToBytes(string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }

        private string Format(int value)
        {
            return char.ToString((char)value);
        }

        
    }
}