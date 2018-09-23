using HomeCenter.Core.Interface.Native;
using HomeCenter.Model.Messages.Commands.Serial;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.ValueTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Core.Services
{
    public class SerialMessagingService : Actor
    {
        private IBinaryReader _dataReader;

        private readonly ISerialDevice _serialDevice;
        private readonly Dictionary<int, SerialRegistrationCommand> _messageHandlers = new Dictionary<int, SerialRegistrationCommand>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private readonly ILogger<SerialMessagingService> _logger;

        public SerialMessagingService(ISerialDevice serialDevice, ILogger<SerialMessagingService> logger)
        {
            _serialDevice = serialDevice ?? throw new ArgumentNullException(nameof(serialDevice));
            _logger = logger;
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            await _serialDevice.Init().ConfigureAwait(false);
            _dataReader = _serialDevice.GetBinaryReader();
            _disposeContainer.Add(_dataReader, _serialDevice);

            var task = Task.Run(async () => await Listen().ConfigureAwait(false), _disposeContainer.Token);
        }

        protected Task Handle(SerialRegistrationCommand registration)
        {
            if (_messageHandlers.ContainsKey(registration.MessageType))
            {
                throw new MessageAlreadyRegistredException($"Message type {registration.MessageType} is already registered in {nameof(SerialMessagingService)}");
            }

            _messageHandlers.Add(registration.MessageType, registration);

            return Task.CompletedTask;
        }

        private async Task Listen()
        {
            while (true)
            {
                try
                {
                    await ReadAsync(_disposeContainer.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception while listening for Serial device");
                }
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            const uint messageHeaderSize = 2;
            cancellationToken.ThrowIfCancellationRequested();

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var headerBytesRead = await _dataReader.LoadAsync(messageHeaderSize, childCancellationTokenSource.Token).ConfigureAwait(false);
                if (headerBytesRead > 0)
                {
                    var messageBodySize = _dataReader.ReadByte();
                    var messageType = _dataReader.ReadByte();
                    var bodyBytesReaded = await _dataReader.LoadAsync(messageBodySize, childCancellationTokenSource.Token).ConfigureAwait(false);

                    if (bodyBytesReaded > 0)
                    {
                        if (!_messageHandlers.TryGetValue(messageType, out SerialRegistrationCommand registration))
                        {
                            throw new UnsupportedMessageException($"Message type {messageType} is not supported by {nameof(SerialMessagingService)}");
                        }

                        if (messageBodySize != registration.MessageSize) throw new UnsupportedMessageException($"Message type {messageType} have wrong size");
                        var result = ReadData(registration);

                        Proto.RootContext.Empty.Send(registration.Actor, result);
                    }
                }
            }
        }

        private SerialResultCommand ReadData(SerialRegistrationCommand registration)
        {
            var result = new SerialResultCommand();

            foreach (var format in registration.ResultFormat.OrderBy(l => l.Lp))
            {
                if (format.ValueType == typeof(byte))
                {
                    result[format.ValueName] = (ByteValue)_dataReader.ReadByte();
                }
                else if (format.ValueType == typeof(uint))
                {
                    result[format.ValueName] = (UIntValue)_dataReader.ReadUInt32();
                }
                else if (format.ValueType == typeof(float))
                {
                    result[format.ValueName] = (DoubleValue)_dataReader.ReadSingle();
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