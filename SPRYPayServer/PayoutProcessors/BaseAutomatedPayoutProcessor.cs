using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Data;
using SPRYPayServer.Data.Data;
using SPRYPayServer.HostedServices;
using SPRYPayServer.Payments;
using SPRYPayServer.PayoutProcessors.Settings;
using SPRYPayServer.Services.Invoices;
using SPRYPayServer.Services.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PayoutData = SPRYPayServer.Data.PayoutData;
using PayoutProcessorData = SPRYPayServer.Data.Data.PayoutProcessorData;

namespace SPRYPayServer.PayoutProcessors;

public abstract class BaseAutomatedPayoutProcessor<T> : BaseAsyncService where T : AutomatedPayoutBlob
{
    protected readonly StoreRepository _storeRepository;
    protected readonly PayoutProcessorData _PayoutProcesserSettings;
    protected readonly ApplicationDbContextFactory _applicationDbContextFactory;
    private readonly PullPaymentHostedService _pullPaymentHostedService;
    protected readonly SPRYPayNetworkProvider _btcPayNetworkProvider;
    protected readonly PaymentMethodId PaymentMethodId;

    protected BaseAutomatedPayoutProcessor(
        ILoggerFactory logger,
        StoreRepository storeRepository,
        PayoutProcessorData payoutProcesserSettings,
        ApplicationDbContextFactory applicationDbContextFactory,
        PullPaymentHostedService pullPaymentHostedService,
        SPRYPayNetworkProvider btcPayNetworkProvider) : base(logger.CreateLogger($"{payoutProcesserSettings.Processor}:{payoutProcesserSettings.StoreId}:{payoutProcesserSettings.PaymentMethod}"))
    {
        _storeRepository = storeRepository;
        _PayoutProcesserSettings = payoutProcesserSettings;
        PaymentMethodId = _PayoutProcesserSettings.GetPaymentMethodId();
        _applicationDbContextFactory = applicationDbContextFactory;
        _pullPaymentHostedService = pullPaymentHostedService;
        _btcPayNetworkProvider = btcPayNetworkProvider;
        this.NoLogsOnExit = true;
    }

    internal override Task[] InitializeTasks()
    {
        return new[] { CreateLoopTask(Act) };
    }

    protected abstract Task Process(ISupportedPaymentMethod paymentMethod, List<PayoutData> payouts);

    private async Task Act()
    {
        var store = await _storeRepository.FindStore(_PayoutProcesserSettings.StoreId);
        var paymentMethod = store?.GetEnabledPaymentMethods(_btcPayNetworkProvider)?.FirstOrDefault(
            method =>
                method.PaymentId == PaymentMethodId);

        var blob = GetBlob(_PayoutProcesserSettings);
        if (paymentMethod is not null)
        {

            await using var context = _applicationDbContextFactory.CreateContext();
            var payouts = await PullPaymentHostedService.GetPayouts(
                new PullPaymentHostedService.PayoutQuery()
                {
                    States = new[] { PayoutState.AwaitingPayment },
                    PaymentMethods = new[] { _PayoutProcesserSettings.PaymentMethod },
                    Stores = new[] { _PayoutProcesserSettings.StoreId }
                }, context);
            if (payouts.Any())
            {
                Logs.PayServer.LogInformation($"{payouts.Count} found to process. Starting (and after will sleep for {blob.Interval})");
                await Process(paymentMethod, payouts);
                await context.SaveChangesAsync();
            }
        }
        await Task.Delay(blob.Interval, CancellationToken);
    }


    public static T GetBlob(PayoutProcessorData data)
    {
        return InvoiceRepository.FromBytes<T>(data.Blob);
    }
}
