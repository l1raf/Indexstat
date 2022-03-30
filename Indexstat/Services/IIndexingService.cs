using Indexstat.DTOs;

namespace Indexstat.Services;

public interface IIndexingService
{
    Task<(string? error, GoogleIndexingStatusResponse? response)> GetGoogleIndexingStatus(Uri uri);

    Task<(string? error, YandexIndexingStatusResponse? response)> GetYandexIndexingStatus(Uri uri);
    
    Task<(string? error, string? source)> GetPageSource(Uri uri, Uri cssFileAddress);
}