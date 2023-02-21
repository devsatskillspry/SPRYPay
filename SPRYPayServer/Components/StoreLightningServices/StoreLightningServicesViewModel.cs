using System.Collections.Generic;
using SPRYPayServer.Data;
using SPRYPayServer.Lightning;
using SPRYPayServer.Models;
using SPRYPayServer.Models.StoreViewModels;

namespace SPRYPayServer.Components.StoreLightningServices;

public class StoreLightningServicesViewModel
{
    public string CryptoCode { get; set; }
    public StoreData Store { get; set; }
    public LightningNodeType LightningNodeType { get; set; }
    public List<AdditionalServiceViewModel> Services { get; set; }
}
