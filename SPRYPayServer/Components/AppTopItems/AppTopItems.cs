using System;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Data;
using SPRYPayServer.Services.Apps;
using SPRYPayServer.Services.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SPRYPayServer.Components.AppTopItems;

public class AppTopItems : ViewComponent
{
    private readonly AppService _appService;

    public AppTopItems(AppService appService)
    {
        _appService = appService;
    }

    public async Task<IViewComponentResult> InvokeAsync(AppTopItemsViewModel vm)
    {
        if (vm.App == null)
            throw new ArgumentNullException(nameof(vm.App));
        if (vm.InitialRendering)
            return View(vm);

        var entries = Enum.Parse<AppType>(vm.App.AppType) == AppType.Crowdfund
            ? await _appService.GetPerkStats(vm.App)
            : await _appService.GetItemStats(vm.App);

        vm.Entries = entries.ToList();

        return View(vm);
    }
}
