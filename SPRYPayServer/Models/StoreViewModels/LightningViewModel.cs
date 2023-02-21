using System.Collections.Generic;

namespace SPRYPayServer.Models.StoreViewModels;

public class LightningViewModel : LightningNodeViewModel
{
    public List<AdditionalServiceViewModel> Services { get; set; }
}
