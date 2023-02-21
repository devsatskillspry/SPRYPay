using System.Collections.Generic;
using SPRYPayServer.Lightning;

namespace SPRYPayServer.Configuration
{
    public class LightningNetworkOptions
    {
        public Dictionary<string, LightningConnectionString> InternalLightningByCryptoCode { get; set; } =
            new Dictionary<string, LightningConnectionString>();
    }
}
