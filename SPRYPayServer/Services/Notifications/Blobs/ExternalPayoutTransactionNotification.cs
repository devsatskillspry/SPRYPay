using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Client.Models;
using SPRYPayServer.Configuration;
using SPRYPayServer.Controllers;
using Microsoft.AspNetCore.Routing;

namespace SPRYPayServer.Services.Notifications.Blobs
{
    public class ExternalPayoutTransactionNotification : BaseNotification
    {
        private const string TYPE = "external-payout-transaction";

        internal class Handler : NotificationHandler<ExternalPayoutTransactionNotification>
        {
            private readonly LinkGenerator _linkGenerator;
            private readonly SPRYPayServerOptions _options;

            public Handler(LinkGenerator linkGenerator, SPRYPayServerOptions options)
            {
                _linkGenerator = linkGenerator;
                _options = options;
            }

            public override string NotificationType => TYPE;

            public override (string identifier, string name)[] Meta
            {
                get
                {
                    return new (string identifier, string name)[] { (TYPE, "External payout approval") };
                }
            }

            protected override void FillViewModel(ExternalPayoutTransactionNotification notification,
                NotificationViewModel vm)
            {
                vm.Identifier = notification.Identifier;
                vm.Type = notification.NotificationType;
                vm.Body =
                    "A payment that was made to an approved payout by an external wallet is waiting for your confirmation.";
                vm.ActionLink = _linkGenerator.GetPathByAction(nameof(UIStorePullPaymentsController.Payouts),
                    "UIStorePullPayments",
                    new
                    {
                        storeId = notification.StoreId,
                        paymentMethodId = notification.PaymentMethod,
                        payoutState = PayoutState.AwaitingPayment
                    }, _options.RootPath);
            }
        }

        public string PayoutId { get; set; }
        public string StoreId { get; set; }
        public string PaymentMethod { get; set; }
        public override string Identifier => TYPE;
        public override string NotificationType => TYPE;
    }
}
