using System.Collections.Generic;
using SPRYPayServer.Data;

namespace SPRYPayServer.Models.ServerViewModels
{
    public class ViewFilesViewModel
    {
        public List<StoredFile> Files { get; set; }
        public Dictionary<string, string> DirectUrlByFiles { get; set; }
        public bool StorageConfigured { get; set; }
    }
}
