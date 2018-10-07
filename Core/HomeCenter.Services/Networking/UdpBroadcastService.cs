using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HomeCenter.Services.Networking
{
    public class UdpBroadcastService : Service
    {

        public UdpBroadcastService(IEventAggregator eventAggregator) : base(eventAggregator)
        {

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