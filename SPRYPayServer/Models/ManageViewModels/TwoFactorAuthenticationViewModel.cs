using System.Collections.Generic;
using SPRYPayServer.Data;

namespace SPRYPayServer.Models.ManageViewModels
{
    public class TwoFactorAuthenticationViewModel
    {

        public int RecoveryCodesLeft { get; set; }

        public bool Is2faEnabled { get; set; }

        public List<Fido2Credential> Credentials { get; set; }

        public string LoginCode { get; set; }
    }
}
