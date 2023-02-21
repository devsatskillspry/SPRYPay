using System;
using System.Threading;
using System.Threading.Tasks;
using SPRYPayServer.Services.Invoices;
using Microsoft.Extensions.Hosting;
using NBitcoin.RPC;

namespace SPRYPayServer.Services
{
    public class Cheater : IHostedService
    {
        private readonly InvoiceRepository _invoiceRepository;
        public RPCClient CashCow { get; set; }

        public Cheater(
            ExplorerClientProvider prov,
            InvoiceRepository invoiceRepository)
        {
            CashCow = prov.GetExplorerClient("SPRY")?.RPCClient;
            _invoiceRepository = invoiceRepository;
        }

        public async Task UpdateInvoiceExpiry(string invoiceId, TimeSpan seconds)
        {
            await _invoiceRepository.UpdateInvoiceExpiry(invoiceId, seconds);
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _ = CashCow?.ScanRPCCapabilitiesAsync(cancellationToken);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
