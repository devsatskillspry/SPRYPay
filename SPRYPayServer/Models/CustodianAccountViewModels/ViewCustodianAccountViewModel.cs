using SPRYPayServer.Abstractions.Custodians;
using SPRYPayServer.Data;

namespace SPRYPayServer.Models.CustodianAccountViewModels
{
    public class ViewCustodianAccountViewModel
    {
        public ICustodian Custodian { get; set; }
        public CustodianAccountData CustodianAccount { get; set; }

    }
}
