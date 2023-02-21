using System.Net;
using System.Net.Http;
using SPRYPayServer.Configuration;
using SPRYPayServer.HostedServices;

namespace SPRYPayServer.Services
{
    public class Socks5HttpClientHandler : HttpClientHandler
    {
        public Socks5HttpClientHandler(SPRYPayServerOptions opts)
        {
            if (opts.SocksEndpoint is IPEndPoint endpoint)
            {
                this.Proxy = new WebProxy($"socks5://{endpoint.Address}:{endpoint.Port}");
            }
            else if (opts.SocksEndpoint is DnsEndPoint endpoint2)
            {
                this.Proxy = new WebProxy($"socks5://{endpoint2.Host}:{endpoint2.Port}");
            }
        }
    }
}
