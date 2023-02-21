using System;
using System.Net.Http;
using SPRYPayServer.Lightning;

namespace SPRYPayServer.Services
{
    public class LightningClientFactoryService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LightningClientFactoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public static string OnionNamedClient { get; set; } = "lightning.onion";

        public ILightningClient Create(LightningConnectionString lightningConnectionString, SPRYPayNetwork network)
        {
            ArgumentNullException.ThrowIfNull(lightningConnectionString);
            ArgumentNullException.ThrowIfNull(network);

            return new LightningClientFactory(network.NBitcoinNetwork)
            {
                HttpClient = _httpClientFactory.CreateClient(lightningConnectionString.BaseUri.IsOnion()
                    ? OnionNamedClient
                    : $"{network.CryptoCode}: Lightning client")
            }.Create(lightningConnectionString);
        }
    }
}
