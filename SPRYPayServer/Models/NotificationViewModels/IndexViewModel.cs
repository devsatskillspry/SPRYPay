using System.Collections.Generic;
using SPRYPayServer.Abstractions.Contracts;

namespace SPRYPayServer.Models.NotificationViewModels
{
    public class IndexViewModel : BasePagingViewModel
    {
        public List<NotificationViewModel> Items { get; set; }

        public override int CurrentPageCount => Items.Count;
    }
}
