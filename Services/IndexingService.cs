using System.Text.RegularExpressions;
using Indexstat.DTOs;
using Indexstat.Enums;
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
        catch (Exception)
        {
            return ("Error fetching search results", null);
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
        catch (Exception)
        {
            return ("Error fetching search results", null);
        }
    }

    public async Task<(string?, string?, string?)> GetPageSource(Uri uri, SearchEngine engine,
        string noindexColor, string nofollowColor)
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

            //Trying to be good
            if (!ShowInsideIframe(response))
                return (null, "Страница не может быть отображена.", "text/html; charset=utf-8");

            data = await response.Content.ReadAsStringAsync();

            data = Regex.Replace(data, @"<(\s*)head(\s*)>", $"<head><base href=\"{uri}\"/>");
            data = Regex.Replace(data, @"<(\s*)/head(\s*)>", $"{GetStyles(engine, noindexColor, nofollowColor)}</head>");

            if (engine == SearchEngine.Yandex)
            {
                data = Regex.Replace(data, @"<!--(\s*)noindex(\s*)-->", "<noindex>");
                data = Regex.Replace(data, @"<!--(\s*)/noindex(\s*)-->", "</noindex>");
            }

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

    private string GetStyles(SearchEngine engine, string? noindexColor, string? nofollowColor)
    {
        noindexColor ??= "#4BFB03";
        nofollowColor ??= "#03defb";

        return engine switch
        {
            //Google ignores <noindex>
            SearchEngine.Google => "<style type=\"text/css\">" +
                                   "*[ref~=\"nofollow\"], *[rel~=\"nofollow\"] {" +
                                   $"background-color: {nofollowColor};" + "}" +
                                   "</style>",
            SearchEngine.Yandex => "<style type=\"text/css\">" +
                                   "noindex, noindex p, noindex a, noindex div {" +
                                   $"background-color: {noindexColor};" + $"background: {noindexColor};" + "}" +
                                   "*[ref~=\"nofollow\"], *[rel~=\"nofollow\"] {" +
                                   $"background-color: {nofollowColor};" + "}" +
                                   "</style>",
            _ => string.Empty
        };
    }

    private bool ShowInsideIframe(HttpResponseMessage response)
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