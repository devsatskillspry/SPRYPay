using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPRYPayServer.Events
{
    public interface IHasInvoiceId
    {
        string InvoiceId { get; }
    }
}
