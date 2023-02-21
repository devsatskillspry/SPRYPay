using System.Collections.Generic;
using SPRYPayServer.Data;

namespace SPRYPayServer.Fido2.Models
{
    public class Fido2AuthenticationViewModel
    {
        public List<Fido2Credential> Credentials { get; set; }
    }
}
