using System.Threading.Tasks;
using SPRYPayServer.Storage.Models;
using SPRYPayServer.Storage.Services.Providers.AzureBlobStorage.Configuration;
using TwentyTwenty.Storage;
using TwentyTwenty.Storage.Azure;

namespace SPRYPayServer.Storage.Services.Providers.AzureBlobStorage
{
    public class
        AzureBlobStorageFileProviderService : BaseTwentyTwentyStorageFileProviderServiceBase<
            AzureBlobStorageConfiguration>
    {
        public override StorageProvider StorageProvider()
        {
            return Storage.Models.StorageProvider.AzureBlobStorage;
        }
        protected override Task<IStorageProvider> GetStorageProvider(AzureBlobStorageConfiguration configuration)
        {
            return Task.FromResult<IStorageProvider>(new AzureStorageProvider(configuration));
        }
    }
}
