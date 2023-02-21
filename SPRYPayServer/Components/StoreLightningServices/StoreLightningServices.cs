using System;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Extensions;
using SPRYPayServer.Configuration;
using SPRYPayServer.Data;
using SPRYPayServer.Lightning;
using SPRYPayServer.Models;
using SPRYPayServer.Models.StoreViewModels;
using SPRYPayServer.Payments;
using SPRYPayServer.Payments.Lightning;
using SPRYPayServer.Services;
using SPRYPayServer.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SPRYPayServer.Components.StoreLightningServices;

public class StoreLightningServices : ViewComponent
{
    private readonly SPRYPayServerOptions _sprypayServerOptions;
    private readonly SPRYPayNetworkProvider _networkProvider;
    private readonly IOptions<ExternalServicesOptions> _externalServiceOptions;

    public StoreLightningServices(
        SPRYPayNetworkProvider networkProvider,
        SPRYPayServerOptions sprypayServerOptions,
        IOptions<ExternalServicesOptions> externalServiceOptions)
    {
        _networkProvider = networkProvider;
        _sprypayServerOptions = sprypayServerOptions;
        _externalServiceOptions = externalServiceOptions;
    }

    public IViewComponentResult Invoke(StoreLightningServicesViewModel vm)
    {
        if (vm.Store == null)
            throw new ArgumentNullException(nameof(vm.Store));
        if (vm.CryptoCode == null)
            throw new ArgumentNullException(nameof(vm.CryptoCode));
        if (vm.LightningNodeType != LightningNodeType.Internal)
            return View(vm);
        if (!User.IsInRole(Roles.ServerAdmin))
            return View(vm);

        var services = _externalServiceOptions.Value.ExternalServices.ToList()
            .Where(service => ExternalServices.LightningServiceTypes.Contains(service.Type))
            .Select(async service =>
            {
                var model = new AdditionalServiceViewModel
                {
                    DisplayName = service.DisplayName,
                    ServiceName = service.ServiceName,
                    CryptoCode = service.CryptoCode,
                    Type = service.Type.ToString()
                };
                try
                {
                    model.Link = await service.GetLink(Request.GetAbsoluteUriNoPathBase(), _sprypayServerOptions.NetworkType);
                }
                catch (Exception exception)
                {
                    model.Error = exception.Message;
                }
                return model;
            })
            .Select(t => t.Result)
            .ToList();

        // other services
        foreach ((string key, Uri value) in _externalServiceOptions.Value.OtherExternalServices)
        {
            if (ExternalServices.LightningServiceNames.Contains(key))
            {
                services.Add(new AdditionalServiceViewModel
                {
                    DisplayName = key,
                    ServiceName = key,
                    Type = key.Replace(" ", ""),
                    Link = Request.GetAbsoluteUriNoPathBase(value).AbsoluteUri
                });
            }
        }

        vm.Services = services;

        return View(vm);
    }
}
