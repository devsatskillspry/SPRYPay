using Fido2NetLib;

namespace SPRYPayServer.Fido2.Models
{
    public class LoginWithFido2ViewModel
    {
        public string UserId { get; set; }

        public bool RememberMe { get; set; }
        public AssertionOptions Data { get; set; }
        public string Response { get; set; }
    }
}
