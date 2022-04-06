using Indexstat.DTOs;
using Indexstat.Enums;

namespace Indexstat.Services;

public interface IIndexingService
{
    Task<(string? error, GoogleIndexingStatusResponse? response)> GetGoogleIndexingStatus(Uri uri);

    Task<(string? error, YandexIndexingStatusResponse? response)> GetYandexIndexingStatus(Uri uri);

    Task<(string? error, string? source, string? contentType)> GetPageSource(Uri uri, SearchEngine engine,
        string noindexColor, string nofollowColor);
}