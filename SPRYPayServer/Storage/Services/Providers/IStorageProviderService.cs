#nullable enable
using System;
using System.Threading.Tasks;
using SPRYPayServer.Data;
using SPRYPayServer.Storage.Models;
using Microsoft.AspNetCore.Http;
using TwentyTwenty.Storage;

namespace SPRYPayServer.Storage.Services.Providers
{
    public interface IStorageProviderService
    {
        Task<StoredFile> AddFile(IFormFile formFile, StorageSettings configuration);
        Task RemoveFile(StoredFile storedFile, StorageSettings configuration);
        Task<string> GetFileUrl(Uri baseUri, StoredFile storedFile, StorageSettings configuration);
        Task<string> GetTemporaryFileUrl(Uri baseUri, StoredFile storedFile, StorageSettings configuration,
            DateTimeOffset expiry, bool isDownload, BlobUrlAccess access = BlobUrlAccess.Read);
        StorageProvider StorageProvider();
    }
}
