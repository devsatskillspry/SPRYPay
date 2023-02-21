using System.Collections.Generic;
using SPRYPayServer.Abstractions.Contracts;

namespace SPRYPayServer.Components.Notifications
{
    public class NotificationsViewModel
    {
        public string ReturnUrl { get; set; }
        public int UnseenCount { get; set; }
        public List<NotificationViewModel> Last5 { get; set; }
    }
}
