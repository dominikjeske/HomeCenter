using HomeCenter.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Queries.Services;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public class TcpMessagingService : Service
    {
        [Subscribe(true)]
        protected async Task Handle(TcpCommand tcpCommand)
        {
            using (var socket = new TcpClient())
            {
                var uri = new Uri($"tcp://{tcpCommand.Address}");
                await socket.ConnectAsync(uri.Host, uri.Port);
                using (var stream = socket.GetStream())
                {
                    await stream.WriteAsync(tcpCommand.Body, 0, tcpCommand.Body.Length);
                }
            }
        }

        [Subscribe(true)]
        protected async Task<string> Handle(TcpQuery tcpCommand)
        {
            using (var socket = new TcpClient())
            {
                var uri = new Uri($"tcp://{tcpCommand.Address}");
                await socket.ConnectAsync(uri.Host, uri.Port);
                using (var stream = socket.GetStream())
                {
                    await stream.WriteAsync(tcpCommand.Body, 0, tcpCommand.Body.Length);
                    return await ReadString(stream);
                }
            }
        }

        private static async Task<string> ReadString(NetworkStream stream)
        {
            var bytesRead = 0;
            var buffer = new byte[256];
            var result = new List<byte>();

            do
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                result.AddRange(buffer.Take(bytesRead));
            }
            while (bytesRead == buffer.Length);

            return System.Text.Encoding.UTF8.GetString(result.ToArray());
        }
    }
}