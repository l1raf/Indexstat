using Indexstat.DTOs;
using Indexstat.SerpApi;

namespace Indexstat.Services;

public class IndexingService : IIndexingService
{
    private readonly HttpClient _httpClient;
    private readonly ISerpApiSearch _search;

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
            var totalSearchResult = await _search.GetYandexSearchResult($"url:{uri}");

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

    public async Task<(string? error, string? source, string? contentType)> GetPageSource(Uri uri, Uri cssFileAddress)
    {
        string? data = null;

        try
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");

            var response = await _httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            if (!CanShowInsideIframe(response))
                return (null, "Страница не может быть отображена.", "text/html; charset=utf-8");

            data = await response.Content.ReadAsStringAsync();

            data = data.IndexOf("</title>", StringComparison.Ordinal) < 0
                ? data.Replace("<head>", $"<head><base href=\"{uri}\"/>")
                : data.Replace("</title>", $"</title><base href=\"{uri}\"/>");

            data = data.Replace("</head>",
                $"<link rel=\"stylesheet\" href=\"{cssFileAddress}\"/></head>");

            data = data.Replace("<!--noindex-->", "<noindex>");
            data = data.Replace("<!--/noindex-->", "</noindex>");
            data = data.Replace("<!-- noindex -->", "<noindex>");
            data = data.Replace("<!-- /noindex -->", "</noindex>");

            return (
                null,
                data,
                response.Content.Headers.TryGetValues("Content-Type", out var contentTypeHeaders)
                    ? contentTypeHeaders.First()
                    : "text/html; charset=utf-8"
            );
        }
        catch (Exception)
        {
            return ("Error loading page", data, null);
        }
    }

    private bool CanShowInsideIframe(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-Frame-Options", out var frameOptions))
            return !frameOptions.Intersect(
                    new[] {"deny", "sameorigin"},
                    StringComparer.OrdinalIgnoreCase)
                .Any();

        if (response.Headers.TryGetValues("Content-Security-Policy", out var ancestorsOptions))
            return !ancestorsOptions.Any(x =>
                x.StartsWith("frame-ancestors", StringComparison.OrdinalIgnoreCase) &&
                (x.Contains("self", StringComparison.OrdinalIgnoreCase) ||
                 x.Contains("none", StringComparison.OrdinalIgnoreCase)));

        return true;
    }
}