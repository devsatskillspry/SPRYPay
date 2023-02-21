using SPRYPayServer.Storage.Services.Providers.Models;
using TwentyTwenty.Storage.Amazon;

namespace SPRYPayServer.Storage.Services.Providers.AmazonS3Storage.Configuration
{
    public class AmazonS3StorageConfiguration : AmazonProviderOptions, IBaseStorageConfiguration
    {
        public string ContainerName { get; set; }
    }
}
