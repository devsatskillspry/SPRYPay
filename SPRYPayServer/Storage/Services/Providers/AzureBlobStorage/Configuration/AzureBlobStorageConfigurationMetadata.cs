using System.ComponentModel.DataAnnotations;

namespace SPRYPayServer.Storage.Services.Providers.AzureBlobStorage.Configuration
{
    public class AzureBlobStorageConfigurationMetadata
    {
        [Required]
        [AzureBlobStorageConnectionStringValidator]
        public string ConnectionString { get; set; }
    }
}
