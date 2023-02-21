using System.ComponentModel.DataAnnotations;
using SPRYPayServer.Services.Mails;
using SPRYPayServer.Validation;

namespace SPRYPayServer.Models.ServerViewModels
{
    public class EmailsViewModel
    {
        public EmailsViewModel()
        {

        }
        public EmailsViewModel(EmailSettings settings)
        {
            Settings = settings;
            PasswordSet = !string.IsNullOrEmpty(settings?.Password);
        }
        public EmailSettings Settings
        {
            get; set;
        }
        public bool PasswordSet { get; set; }
        [MailboxAddressAttribute]
        [Display(Name = "Test Email")]
        public string TestEmail
        {
            get; set;
        }
    }
}
