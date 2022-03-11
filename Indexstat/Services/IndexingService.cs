using Indexstat.DTOs;
using Indexstat.SerpApi;

namespace Indexstat.Services;

public class IndexingService : IIndexingService
{
    private readonly ISerpApiSearch _search;
    private readonly HttpClient _httpClient;

    public IndexingService(ISerpApiSearch search, HttpClient httpClient)
    {
        _search = search;
        _httpClient = httpClient;
    }

    public async Task<(string?, GoogleIndexingStatusResponse?)> GetGoogleIndexingStatus(Uri uri)
    {
        try
        {
            var totalSearchResult = await _search.GetGoogleSearchResult($"site:{uri}");

            return (null, new GoogleIndexingStatusResponse
            {
                TotalSearchResults = totalSearchResult
            });
        }
        catch (Exception ex)
        {
            return (ex.Message, null);
        }
    }

    public async Task<(string?, YandexIndexingStatusResponse?)> GetYandexIndexingStatus(Uri uri)
    {
        try
        {
            var totalSearchResult = await _search.GetYandexSearchResult($"site:{uri}");

            return (null, new YandexIndexingStatusResponse
            {
                TotalSearchResults = totalSearchResult
            });
        }
        catch (Exception ex)
        {
            return (ex.Message, null);
        }
    }

    public async Task<(string? error, string? source)> GetPageSource(Uri uri)
    {
        try
        {
            var response = await _httpClient.GetAsync(uri);
            var data = await response.Content.ReadAsStringAsync();

            data = data.Replace("</title>",$"</title><base href=\"{uri}\"/>");
            data = data.Replace("</head>",
                "<link rel=\"stylesheet\" href=\"http://localhost:3000/nonindexed.css\"/></head");

            return (null, data);
        }
        catch (Exception)
        {
            return ("Error loading page", null);
        }
    }
}