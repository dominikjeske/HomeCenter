using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Service;
using HomeCenter.Model.Native;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public abstract class SerialMessagingService : Service
    {
        private readonly ISerialDevice _serialDevice;
        private readonly Dictionary<int, SerialRegistrationCommand> _messageHandlers = new Dictionary<int, SerialRegistrationCommand>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();

        protected SerialMessagingService(ISerialDevice serialDevice)
        {
            _serialDevice = serialDevice ?? throw new ArgumentNullException(nameof(serialDevice));
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _serialDevice.Init();
            _disposeContainer.Add(_serialDevice);

            //var task = Task.Run(async () => await Listen().ConfigureAwait(false), _disposeContainer.Token);
        }

        [Subscibe]
        protected Task Handle(SerialRegistrationCommand registration)
        {
            if (_messageHandlers.ContainsKey(registration.MessageType))
            {
                throw new MessageAlreadyRegistredException($"Message type {registration.MessageType} is already registered in {nameof(SerialMessagingService)}");
            }

            _messageHandlers.Add(registration.MessageType, registration);

            return Task.CompletedTask;
        }

        //private async Task Listen()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            await ReadAsync(_disposeContainer.Token).ConfigureAwait(false);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.LogError(ex, "Exception while listening for Serial device");
        //        }
        //    }
        //}

        //private async Task ReadAsync(CancellationToken cancellationToken)
        //{
        //    const uint messageHeaderSize = 2;
        //    cancellationToken.ThrowIfCancellationRequested();

        //    using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        //    {
        //        var headerBytesRead = await _dataReader.LoadAsync(messageHeaderSize, childCancellationTokenSource.Token).ConfigureAwait(false);
        //        if (headerBytesRead > 0)
        //        {
        //            var messageBodySize = _dataReader.ReadByte();
        //            var messageType = _dataReader.ReadByte();
        //            var bodyBytesReaded = await _dataReader.LoadAsync(messageBodySize, childCancellationTokenSource.Token).ConfigureAwait(false);

        //            if (bodyBytesReaded > 0)
        //            {
        //                if (!_messageHandlers.TryGetValue(messageType, out SerialRegistrationCommand registration))
        //                {
        //                    throw new UnsupportedMessageException($"Message type {messageType} is not supported by {nameof(SerialMessagingService)}");
        //                }

        //                if (messageBodySize != registration.MessageSize) throw new UnsupportedMessageException($"Message type {messageType} have wrong size");
        //                var result = ReadData(registration.ResultFormat);

        //                MessageBroker.Send(result, registration.Actor);
        //            }
        //        }
        //    }
        //}

    //    private SerialResultEvent ReadData(Format[] registration)
    //    {
    //        var result = new SerialResultEvent();

    //        foreach (var format in registration.OrderBy(l => l.Lp))
    //        {
    //            if (format.ValueType == typeof(byte))
    //            {
    //                result.SetProperty(format.ValueName, _dataReader.ReadByte());
    //            }
    //            else if (format.ValueType == typeof(uint))
    //            {
    //                result.SetProperty(format.ValueName, _dataReader.ReadUInt32());
    //            }
    //            else if (format.ValueType == typeof(float))
    //            {
    //                result.SetProperty(format.ValueName, _dataReader.ReadSingle());
    //            }
    //            else
    //            {
    //                throw new UnsupportedResultException($"Result of type {format.ValueType} is not supported");
    //            }
    //        }

    //        return result;
    //    }
    }
}