using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Interface.Messaging;

namespace Wirehome.Core.Services
{
    public class UdpBroadcastService : IUdpBroadcastService
    {
        private readonly ILogger _logService;
        private readonly IEventAggregator _eventAggregator;
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();

        public UdpBroadcastService(ILogService logService, IEventAggregator eventAggregator)
        {
            _logService = logService.CreatePublisher(nameof(UdpBroadcastService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
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
                _logService.Error(ex, $"Message {message.GetType().Name} failed during send UDP broadcast message");
            }
            return null;
        }
    }
}