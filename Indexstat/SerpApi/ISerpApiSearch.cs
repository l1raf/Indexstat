namespace Indexstat.SerpApi;

public interface ISerpApiSearch
{
    Task<int> GetGoogleSearchResult(string query);

    Task<string?> GetYandexSearchResult(string query);
}