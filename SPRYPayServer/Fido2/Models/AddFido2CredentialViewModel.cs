using Fido2NetLib.Objects;

namespace SPRYPayServer.Fido2.Models
{
    public class AddFido2CredentialViewModel
    {
        public AuthenticatorAttachment? AuthenticatorAttachment { get; set; }
        public string Name { get; set; }
    }

}
