using SPRYPayServer.Abstractions.Form;
using SPRYPayServer.Data;

namespace SPRYPayServer.Models.CustodianAccountViewModels
{
    public class EditCustodianAccountViewModel
    {

        public CustodianAccountData CustodianAccount { get; set; }
        public Form ConfigForm { get; set; }
    }
}
