using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Interface.Messaging;
using Microsoft.Extensions.Logging;

namespace HomeCenter.Core.Services
{
    public class UdpBroadcastService : IUdpBroadcastService
    {
        private readonly ILogger<UdpBroadcastService> _logService;
        private readonly IEventAggregator _eventAggregator;
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private readonly ILogger<UdpBroadcastService> _logger;

        public UdpBroadcastService(ILogger<UdpBroadcastService> logger, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = logger;
        }

        public void Dispose() => _disposeContainer.Dispose();

        public Task Initialize()
        {
            _disposeContainer.Add(_eventAggregator.SubscribeForAsyncResult<IUdpBroadcastMessage>(MessageHandler));
            return Task.CompletedTask;
        }

        private Task<object> MessageHandler(IMessageEnvelope<IUdpBroadcastMessage> message)
        {
            return SendMessage(message);
        }

        private async Task<object> SendMessage(IMessageEnvelope<IUdpBroadcastMessage> message)
        {
            try
            {
                using (var socket = new UdpClient())
                {
                    var uri = new Uri($"udp://{message.Message.MessageAddress()}");

                    socket.Connect(uri.Host, uri.Port);
                    socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
                    var messageBytes = message.Message.Serialize();
                    await socket.SendAsync(messageBytes, messageBytes.Length).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError(ex, $"Message {message.GetType().Name} failed during send UDP broadcast message");
            }
            return null;
        }
    }
}