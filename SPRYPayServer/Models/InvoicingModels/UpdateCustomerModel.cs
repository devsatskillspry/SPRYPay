using System.ComponentModel.DataAnnotations;
using SPRYPayServer.Validation;

namespace SPRYPayServer.Models.InvoicingModels
{
    public class UpdateCustomerModel
    {
        [MailboxAddress]
        [Required]
        public string Email
        {
            get; set;
        }
    }
}
