using System.Collections.Generic;
using SPRYPayServer.Storage.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SPRYPayServer.Storage.ViewModels
{
    public class ChooseStorageViewModel
    {
        public IEnumerable<SelectListItem> ProvidersList { get; set; }
        public StorageProvider Provider { get; set; }
        public bool ShowChangeWarning { get; set; }
    }
}
