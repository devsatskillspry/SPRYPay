using System;
using System.Collections.Generic;

namespace SPRYPayServer.Configuration
{
    public class ExternalServicesOptions
    {
        public Dictionary<string, Uri> OtherExternalServices { get; set; } = new Dictionary<string, Uri>();
        public ExternalServices ExternalServices { get; set; } = new ExternalServices();
    }
}
