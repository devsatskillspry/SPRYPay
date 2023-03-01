using System;

namespace SPRYPayServer.Configuration
{
    public class NBXplorerConnectionSetting
    {
        public string CryptoCode { get; internal set; }
        public Uri ExplorerUri { get; internal set; }
        public string CookieFile { get; internal set; }
        public string CryptoNote {get; internal set; }
        public api ExplorerApi {get; internal set; }
    }
}
