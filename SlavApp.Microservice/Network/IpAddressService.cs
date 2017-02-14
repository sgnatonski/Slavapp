using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Network
{
    public class IpAddressService
    {
        private string localIp;
        public string GetLocalAddress()
        {
            if (localIp == null)
            {
                var ips = NetworkInterface.GetAllNetworkInterfaces()
                    .SelectMany(adapter => adapter.GetIPProperties().UnicastAddresses)
                    .Where(adr => adr.Address.AddressFamily == AddressFamily.InterNetwork && adr.IsDnsEligible)
                    .Select(adr => adr.Address.ToString());
                localIp = ips.FirstOrDefault(x => x.StartsWith("192.")) ?? "127.0.0.1";
            }
            return localIp;
        }
    }
}
