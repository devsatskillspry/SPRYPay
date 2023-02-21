using SPRYPayServer.Storage.Services.Providers.Models;

namespace SPRYPayServer.Storage.Services.Providers.FileSystemStorage.Configuration
{
    public class FileSystemStorageConfiguration : IBaseStorageConfiguration
    {
        public string ContainerName { get; set; } = string.Empty;
    }
}
