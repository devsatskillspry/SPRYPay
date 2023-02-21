#nullable enable
using SPRYPayServer.Abstractions.Contracts;
using Microsoft.AspNetCore.Http;

namespace SPRYPayServer.Services.Stores;

public class ScopeProvider : IScopeProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ScopeProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string? GetCurrentStoreId()
    {
        return _httpContextAccessor.HttpContext.GetStoreData()?.Id;
    }
}
