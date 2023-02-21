using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Controllers;
using SPRYPayServer.Data;
using SPRYPayServer.Models.StoreViewModels;
using SPRYPayServer.Payments;
using SPRYPayServer.Payments.Lightning;
using SPRYPayServer.Services;
using SPRYPayServer.Services.Apps;
using SPRYPayServer.Services.Custodian.Client;
using SPRYPayServer.Services.Invoices;
using SPRYPayServer.Services.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NBitcoin;
using NBitcoin.Secp256k1;

namespace SPRYPayServer.Components.MainNav
{
    public class MainNav : ViewComponent
    {
        private readonly AppService _appService;
        private readonly StoreRepository _storeRepo;
        private readonly UIStoresController _storesController;
        private readonly SPRYPayNetworkProvider _networkProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PaymentMethodHandlerDictionary _paymentMethodHandlerDictionary;
        private readonly CustodianAccountRepository _custodianAccountRepository;

        public MainNav(
            AppService appService,
            StoreRepository storeRepo,
            UIStoresController storesController,
            SPRYPayNetworkProvider networkProvider,
            UserManager<ApplicationUser> userManager,
            PaymentMethodHandlerDictionary paymentMethodHandlerDictionary,
            CustodianAccountRepository custodianAccountRepository,
            PoliciesSettings policiesSettings)
        {
            _storeRepo = storeRepo;
            _appService = appService;
            _userManager = userManager;
            _networkProvider = networkProvider;
            _storesController = storesController;
            _paymentMethodHandlerDictionary = paymentMethodHandlerDictionary;
            _custodianAccountRepository = custodianAccountRepository;
            PoliciesSettings = policiesSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var store = ViewContext.HttpContext.GetStoreData();
            var vm = new MainNavViewModel { Store = store };
#if ALTCOINS
            vm.AltcoinsBuild = true;
#endif
            if (store != null)
            {
                var storeBlob = store.GetStoreBlob();

                // Wallets
                _storesController.AddPaymentMethods(store, storeBlob,
                    out var derivationSchemes, out var lightningNodes);
                vm.DerivationSchemes = derivationSchemes;
                vm.LightningNodes = lightningNodes;

                // Apps
                var apps = await _appService.GetAllApps(UserId, false, store.Id);
                vm.Apps = apps.Select(a => new StoreApp
                {
                    Id = a.Id,
                    IsOwner = a.IsOwner,
                    AppName = a.AppName,
                    AppType = Enum.Parse<AppType>(a.AppType)
                }).ToList();

                if (PoliciesSettings.Experimental)
                {
                    // Custodian Accounts
                    var custodianAccounts = await _custodianAccountRepository.FindByStoreId(store.Id);
                    vm.CustodianAccounts = custodianAccounts;
                }
            }

            return View(vm);
        }

        private string UserId => _userManager.GetUserId(HttpContext.User);

        public PoliciesSettings PoliciesSettings { get; }
    }
}
