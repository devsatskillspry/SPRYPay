using System;

namespace SPRYPayServer.Payments
{
    public class PaymentMethodUnavailableException : Exception
    {
        public PaymentMethodUnavailableException(string message) : base(message)
        {

        }
        public PaymentMethodUnavailableException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
