using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public abstract class SerialPortService : Service
    {
        private readonly ISerialDevice _serialDevice;
        private readonly Dictionary<int, SerialRegistrationCommand> _messageHandlers = new Dictionary<int, SerialRegistrationCommand>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();

        protected SerialPortService(ISerialDevice serialDevice)
        {
            _serialDevice = serialDevice ?? throw new ArgumentNullException(nameof(serialDevice));
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _serialDevice.Init();
            _disposeContainer.Add(_serialDevice.Subscribe(Handle));
            _disposeContainer.Add(_serialDevice);
        }

        [Subscibe]
        protected Task Handle(SerialRegistrationCommand registration)
        {
            if (_messageHandlers.ContainsKey(registration.MessageType))
            {
                throw new MessageAlreadyRegistredException($"Message type {registration.MessageType} is already registered in {nameof(SerialPortService)}");
            }

            _messageHandlers.Add(registration.MessageType, registration);

            return Task.CompletedTask;
        }

        private void Handle(byte[] rawData)
        {
            using (var str = new MemoryStream(rawData))
            using (var reader = new BinaryReader(str))
            {
                var messageBodySize = reader.ReadByte();
                var messageType = reader.ReadByte();

                if (messageType == 0)
                {
                    Logger.LogInformation("Test message from RC");
                }

                if (messageType == 10)
                {
                    var byteArray = reader.ReadBytes(rawData.Length - 2);
                    string message = Encoding.UTF8.GetString(byteArray);
                    Logger.LogInformation(message);
                    return;
                }

                if (!_messageHandlers.TryGetValue(messageType, out SerialRegistrationCommand registration))
                {
                    Logger.LogInformation($"Message type {messageType} is not supported by {nameof(SerialPortService)}");
                    return;
                    //throw new UnsupportedMessageException($"Message type {messageType} is not supported by {nameof(SerialMessagingService)}");
                }

                if (messageBodySize != registration.MessageSize) throw new UnsupportedMessageException($"Message type {messageType} have wrong size");
                var result = ReadData(registration.ResultFormat, reader);

                MessageBroker.Send(result, registration.Actor);
            }
        }

        private SerialResultEvent ReadData(Format[] registration, BinaryReader reader)
        {
            var result = new SerialResultEvent();

            foreach (var format in registration.OrderBy(l => l.Lp))
            {
                if (format.ValueType == typeof(byte))
                {
                    result.SetProperty(format.ValueName, reader.ReadByte());
                }
                else if (format.ValueType == typeof(uint))
                {
                    result.SetProperty(format.ValueName, reader.ReadUInt32());
                }
                else if (format.ValueType == typeof(float))
                {
                    result.SetProperty(format.ValueName, reader.ReadSingle());
                }
                else
                {
                    throw new UnsupportedResultException($"Result of type {format.ValueType} is not supported");
                }
            }

            return result;
        }
    }
}