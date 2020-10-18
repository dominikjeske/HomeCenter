using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace HomeCenter.Utils
{
    public static class NetworkHelper
    {
        public static IEnumerable<IPAddress> GetNetworkAddresses()
        {            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())            {                if (item.OperationalStatus == OperationalStatus.Up)
                {                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)                    {                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork & !IPAddress.IsLoopback(ip.Address))                        {                            yield return ip.Address;                        }                    }                }            }
        }
    }
}