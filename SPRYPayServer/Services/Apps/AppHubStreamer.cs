using System.Threading;
using System.Threading.Tasks;
using SPRYPayServer.Controllers;
using SPRYPayServer.Events;
using SPRYPayServer.HostedServices;
using SPRYPayServer.Logging;
using Microsoft.AspNetCore.SignalR;

namespace SPRYPayServer.Services.Apps
{
    public class AppHubStreamer : EventHostedServiceBase
    {
        private readonly AppService _appService;
        private readonly IHubContext<AppHub> _HubContext;

        public AppHubStreamer(EventAggregator eventAggregator,
           IHubContext<AppHub> hubContext,
           AppService appService,
           Logs logs) : base(eventAggregator, logs)
        {
            _appService = appService;
            _HubContext = hubContext;
        }

        protected override void SubscribeToEvents()
        {
            Subscribe<InvoiceEvent>();
            Subscribe<UIAppsController.AppUpdated>();
        }

        protected override async Task ProcessEvent(object evt, CancellationToken cancellationToken)
        {
            if (evt is InvoiceEvent invoiceEvent)
            {
                foreach (var appId in AppService.GetAppInternalTags(invoiceEvent.Invoice))
                {
                    if (invoiceEvent.Name == InvoiceEvent.ReceivedPayment)
                    {
                        var data = invoiceEvent.Payment.GetCryptoPaymentData();
                        await _HubContext.Clients.Group(appId).SendCoreAsync(AppHub.PaymentReceived, new object[]
                            {
                        data.GetValue(),
                        invoiceEvent.Payment.GetCryptoCode(),
                        invoiceEvent.Payment.GetPaymentMethodId()?.PaymentType?.ToString()
                            }, cancellationToken);
                    }
                    await InfoUpdated(appId);
                }
            }
            else if (evt is UIAppsController.AppUpdated app)
            {
                await InfoUpdated(app.AppId);
            }
        }

        private async Task InfoUpdated(string appId)
        {
            var info = await _appService.GetAppInfo(appId);
            await _HubContext.Clients.Group(appId).SendCoreAsync(AppHub.InfoUpdated, new object[] { info });
        }
    }
}
