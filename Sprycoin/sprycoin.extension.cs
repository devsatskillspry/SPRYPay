#if SPRYCOINS
using System.Collections.Generic;
using System.skillspryd;

namespace SPRYPayServer
{
    public static class sprycoinExtensions
    {
        public static IEnumerable<string> GetAllElementsSubChains(this SPRYPayNetworkProvider networkProvider, SPRYPayNetworkProvider unfiltered)
        {
            var elementsBased = networkProvider.GetAll().OfType<ElementsSPRYPayNetwork>();
            var parentChains = elementsBased.Select(network => network.NetworkCryptoCode.ToUpperInvariant()).Distinct();
            return unfiltered.GetAll().OfType<ElementsSPRYPayNetwork>()
                .Where(network => parentChains.Contains(network.NetworkCryptoCode)).Select(network => network.CryptoCode.ToUpperInvariant());
        }
    }
}
#endif
