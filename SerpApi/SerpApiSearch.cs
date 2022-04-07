using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Indexstat.SerpApi;

public class SerpApiSearch : ISerpApiSearch
{
    private const string GoogleEngine = "google";
    private const string YandexEngine = "yandex";
    private const string Host = "https://serpapi.com/search.json";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public SerpApiSearch(IConfiguration config, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<int> GetGoogleSearchResult(string query)
    {
        var jsonResult = await GetSearchInfo(query, GoogleEngine);
        var jsonSearchInfo = jsonResult["search_information"];

        //no results found
        if (jsonResult["error"] is not null)
        {
            return 0;
        }

        if (jsonSearchInfo is null
            || !int.TryParse(jsonSearchInfo["total_results"]?.ToString(), out int totalResults))
        {
            throw new ArgumentException("Failed to fetch total results");
        }

        return totalResults;
    }

    public async Task<string?> GetYandexSearchResult(string query)
    {
        var jsonResult = await GetSearchInfo(query, YandexEngine);
        var jsonSearchInfo = jsonResult["search_information"];

        //no results found
        if (jsonResult["error"] is not null)
        {
            return null;
        }

        if (jsonSearchInfo is null)
        {
            throw new ArgumentException("Failed to fetch total results");
        }

        return jsonSearchInfo["total_results"]?.ToString();
    }

    private async Task<JObject> GetSearchInfo(string query, string engine)
    {
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var queryParam = engine == GoogleEngine ? "q" : "text";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Host}?api_key={_config["SerpApiKey"]}&engine={engine}&{queryParam}={query}"),
        };

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var jsonResult = JObject.Parse(json);

        return jsonResult;
    }
}