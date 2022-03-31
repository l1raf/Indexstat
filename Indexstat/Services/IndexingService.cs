using System.Text;
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

    public async Task<(string? error, string? source)> GetPageSource(Uri uri, Uri cssFileAddress)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
            // _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            var response = await _httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            string data;

            if (response.Headers.TryGetValues("Transfer-Encoding", out IEnumerable<string>? headerValues)
                && headerValues.Any(x => x.Equals("chunked", StringComparison.InvariantCultureIgnoreCase)))
            {
                data = ReadChunkedResponse(await response.Content.ReadAsStreamAsync());
            }
            else
            {
                data = await response.Content.ReadAsStringAsync();
            }
            
            
            if (data.IndexOf("<base href", StringComparison.Ordinal) < 0)
                data = data.IndexOf("</title>", StringComparison.Ordinal) < 0
                    ? data.Replace("<head>", $"<head><base href=\"{uri}\"/>")
                    : data.Replace("</title>", $"</title><base href=\"{uri}\"/>");

            data = data.Replace("</head>",
                $"<link rel=\"stylesheet\" href=\"{cssFileAddress}\"/></head>");

            data = data.Replace("<!--noindex-->", "<noindex>");
            data = data.Replace("<!--/noindex-->", "</noindex>");

            return (null, data);
        }
        catch (Exception)
        {
            return ("Error loading page", null);
        }
    }

    private string ReadChunkedResponse(Stream responseStream)
    {
        var reader = new StreamReader(responseStream, Encoding.UTF8);
        var data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
        return Encoding.Default.GetString(data.ToArray());
    }
}