using System.ComponentModel.DataAnnotations;
using SPRYPayServer.Storage.Services.Providers.Models;
using Microsoft.AspNetCore.Mvc;
using TwentyTwenty.Storage.Azure;

namespace SPRYPayServer.Storage.Services.Providers.AzureBlobStorage.Configuration
{
    [ModelMetadataType(typeof(AzureBlobStorageConfigurationMetadata))]
    public class AzureBlobStorageConfiguration : AzureProviderOptions, IBaseStorageConfiguration
    {
        [Required]
        [MinLength(3)]
        [MaxLength(63)]
        [RegularExpression(@"[a-z0-9-]+",
            ErrorMessage = "Characters must be lowercase or digits or -")]
        public string ContainerName { get; set; }
    }
}
