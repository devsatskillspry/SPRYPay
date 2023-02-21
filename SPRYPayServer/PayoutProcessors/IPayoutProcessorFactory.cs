using System.Collections.Generic;
using System.Threading.Tasks;
using SPRYPayServer.Data.Data;
using SPRYPayServer.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace SPRYPayServer.PayoutProcessors;

public interface IPayoutProcessorFactory
{
    public string Processor { get; }
    public string FriendlyName { get; }
    public string ConfigureLink(string storeId, PaymentMethodId paymentMethodId, HttpRequest request);
    public IEnumerable<PaymentMethodId> GetSupportedPaymentMethods();
    public Task<IHostedService> ConstructProcessor(PayoutProcessorData settings);
}
