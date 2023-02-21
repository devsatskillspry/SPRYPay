using SPRYPayServer.Data.Data;
using SPRYPayServer.Payments;
using SPRYPayServer.PayoutProcessors.Lightning;
using SPRYPayServer.PayoutProcessors.OnChain;
using Microsoft.Extensions.DependencyInjection;

namespace SPRYPayServer.PayoutProcessors;

public static class PayoutProcessorsExtensions
{
    public static void AddPayoutProcesors(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<OnChainAutomatedPayoutSenderFactory>();
        serviceCollection.AddSingleton<IPayoutProcessorFactory>(provider => provider.GetRequiredService<OnChainAutomatedPayoutSenderFactory>());
        serviceCollection.AddSingleton<LightningAutomatedPayoutSenderFactory>();
        serviceCollection.AddSingleton<IPayoutProcessorFactory>(provider => provider.GetRequiredService<LightningAutomatedPayoutSenderFactory>());
        serviceCollection.AddSingleton<PayoutProcessorService>();
        serviceCollection.AddHostedService(s => s.GetRequiredService<PayoutProcessorService>());
    }

    public static PaymentMethodId GetPaymentMethodId(this PayoutProcessorData data)
    {
        return PaymentMethodId.Parse(data.PaymentMethod);
    }
}
