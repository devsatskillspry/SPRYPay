using System;
using System.Threading.Tasks;
using SPRYPayServer.Data;

namespace SPRYPayServer.Events
{
    public class UserRegisteredEvent
    {
        public ApplicationUser User { get; set; }
        public bool Admin { get; set; }
        public Uri RequestUri { get; set; }

        public TaskCompletionSource<Uri> CallbackUrlGenerated;
    }
}
