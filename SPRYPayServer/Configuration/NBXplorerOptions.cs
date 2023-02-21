using System.Collections.Generic;

namespace SPRYPayServer.Configuration
{
    public class NBXplorerOptions
    {
        public List<NBXplorerConnectionSetting> NBXplorerConnectionSettings
        {
            get;
            set;
        } = new List<NBXplorerConnectionSetting>();
        public string ConnectionString { get; set; }
    }
}
