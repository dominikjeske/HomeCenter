using HomeCenter.Messaging;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HomeCenter.Core.Services
{
    public class UdpBroadcastService : Service
    {
        private readonly ILogger<UdpBroadcastService> _logger;

        public UdpBroadcastService(ILogger<UdpBroadcastService> logger, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _logger = logger;
        }

        [Subscibe]
        protected async Task SendMessage(UdpCommand udpCommand)
        {
            using (var socket = new UdpClient())
            {
                var uri = new Uri($"udp://{udpCommand.Address}");

                socket.Connect(uri.Host, uri.Port);
                socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
                await socket.SendAsync(udpCommand.Body, udpCommand.Body.Length).ConfigureAwait(false);
            }
        }
    }
}