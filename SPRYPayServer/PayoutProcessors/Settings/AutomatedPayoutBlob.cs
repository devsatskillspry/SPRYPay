using System;

namespace SPRYPayServer.PayoutProcessors.Settings;

public class AutomatedPayoutBlob
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);
}
