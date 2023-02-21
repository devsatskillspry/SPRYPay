using System.Collections.Generic;
using SPRYPayServer.Data;
using SPRYPayServer.Services.Apps;

namespace SPRYPayServer.Components.AppSales;

public class AppSalesViewModel
{
    public AppData App { get; set; }
    public AppSalesPeriod Period { get; set; } = AppSalesPeriod.Week;
    public int SalesCount { get; set; }
    public IEnumerable<SalesStatsItem> Series { get; set; }
    public bool InitialRendering { get; set; }
}
