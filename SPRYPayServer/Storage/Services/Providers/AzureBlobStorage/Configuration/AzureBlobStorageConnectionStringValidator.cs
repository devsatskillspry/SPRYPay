using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage;

namespace SPRYPayServer.Storage.Services.Providers.AzureBlobStorage.Configuration
{
    public class AzureBlobStorageConnectionStringValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                CloudStorageAccount.Parse(value as string);
                return ValidationResult.Success;
            }
            catch (Exception e)
            {
                return new ValidationResult(e.Message);
            }
        }
    }
}
