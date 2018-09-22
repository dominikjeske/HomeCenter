using HomeCenter.Core.Interface.Native;
using HomeCenter.Model.Commands.Serial;
using HomeCenter.Model.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Core.Services
{
    public class SerialMessagingService : Actor
    {
        private IBinaryReader _dataReader;

        private readonly ISerialDevice _serialDevice;
        private readonly List<SerialRegistrationCommand> _messageHandlers = new List<SerialRegistrationCommand>();
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

            _disposeContainer.Add(_dataReader);
            _disposeContainer.Add(_serialDevice);

            var task = Task.Run(async () => await Listen().ConfigureAwait(false), _disposeContainer.Token);
        }

        protected Task Handle(SerialRegistrationCommand registration)
        {
            _messageHandlers.Add(registration);

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
                        foreach (var handler in _messageHandlers)
                        {
                            //if (await handler(messageType, messageBodySize, _dataReader).ConfigureAwait(false))
                            //{
                            //    break;
                            //}
                        }
                    }
                }
            }
        }
    }
}