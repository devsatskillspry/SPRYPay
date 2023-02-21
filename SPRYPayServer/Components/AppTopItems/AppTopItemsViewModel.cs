using System.Collections.Generic;
using SPRYPayServer.Data;
using SPRYPayServer.Services.Apps;

namespace SPRYPayServer.Components.AppTopItems;

public class AppTopItemsViewModel
{
    public AppData App { get; set; }
    public List<ItemStats> Entries { get; set; }
    public bool InitialRendering { get; set; }
}
