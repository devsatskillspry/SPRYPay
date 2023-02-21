using System.Collections.Generic;
using SPRYPayServer.Data;
using SPRYPayServer.Models.StoreViewModels;
using SPRYPayServer.Services.Apps;

namespace SPRYPayServer.Components.MainNav
{
    public class MainNavViewModel
    {
        public StoreData Store { get; set; }
        public List<StoreDerivationScheme> DerivationSchemes { get; set; }
        public List<StoreLightningNode> LightningNodes { get; set; }
        public List<StoreApp> Apps { get; set; }
        public CustodianAccountData[] CustodianAccounts { get; set; }
        public bool AltcoinsBuild { get; set; }
    }

    public class StoreApp
    {
        public string Id { get; set; }
        public string AppName { get; set; }
        public AppType AppType { get; set; }
        public bool IsOwner { get; set; }
    }
}
