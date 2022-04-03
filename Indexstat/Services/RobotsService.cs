using Indexstat.DTOs;
using Indexstat.RobotsParser;
using Indexstat.RobotsParser.Models;

namespace Indexstat.Services;

public class RobotsService : IRobotsService
{
    private readonly IRobotsParser _robotsParser;
    private readonly HttpClient _httpClient;

    public RobotsService(IRobotsParser robotsParser, HttpClient httpClient)
    {
        _robotsParser = robotsParser;
        _httpClient = httpClient;
    }

    public async Task<(string? error, RobotsHeadersResponse? headers)> GetRobotsResponseHeaders(Uri uri)
    {
        try
        {
            var headersResponse = new RobotsHeadersResponse();
            
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");

            var response = await _httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();
            
            if (response.Headers.TryGetValues("X-Robots-Tag", out IEnumerable<string>? headerValues))
            {
                headersResponse.Headers = headerValues.Select(x => $"X-Robots-tag: {x}");
            }
            
            return (null, headersResponse);
        }
        catch (Exception e)
        {
            return (e.Message, null);
        }
    }

    public async Task<(string? error, IEnumerable<Uri>? paths)> GetDisallowedToBotUrls(Uri uri, string bot)
    {
        try
        {
            var baseUri = uri.GetLeftPart(UriPartial.Authority);
            return (null, GetDisallowedToBotUrls(await GetRobotsFile(uri), baseUri, bot));
        }
        catch (Exception e)
        {
            return (e.Message, null);
            // return ("Failed to get robots.txt", null);
        }
    }

    public async Task<(string? error, RobotsTxtResponse? paths)> GetDisallowedUrls(Uri uri)
    {
        try
        {
            var baseUri = uri.GetLeftPart(UriPartial.Authority);
            var robotsFile = await GetRobotsFile(uri);

            return (null, new RobotsTxtResponse
            {
                RobotsUri = robotsFile.Uri,
                Google = GetDisallowedToBotUrls(robotsFile, baseUri, "googlebot"),
                Yandex = GetDisallowedToBotUrls(robotsFile, baseUri, "yandex")
            });
        }
        catch (Exception e)
        {
            return (e.Message, null);
            // return ("Failed to get robots.txt", null);
        }
    }

    private async Task<RobotsFile> GetRobotsFile(Uri uri)
    {
        var baseUri = uri.GetLeftPart(UriPartial.Authority);
        return await _robotsParser.FromUriAsync(new Uri($"{baseUri}/robots.txt"));
    }

    private IEnumerable<Uri> GetDisallowedToBotUrls(RobotsFile robotsFile, string baseUri, string bot)
    {
        var siteAccessEntries =
            robotsFile.SiteAccessEntries as SiteAccessEntry[] ?? robotsFile.SiteAccessEntries.ToArray();

        //Yandex ignores user-agent * if yandex is found
        //Same for Google and googlebot
        if (siteAccessEntries.Any(x => x.UserAgents.Contains(bot, StringComparer.InvariantCultureIgnoreCase))
            && siteAccessEntries.Any(x => x.UserAgents.Contains("*")))
        {
            siteAccessEntries = siteAccessEntries
                .Where(x => x.UserAgents
                    .Contains(bot, StringComparer.InvariantCultureIgnoreCase)).ToArray();
        }

        return siteAccessEntries
            .Where(x => x.UserAgents
                .Contains(bot, StringComparer.InvariantCultureIgnoreCase) || x.UserAgents.Contains("*"))
            .Select(entry => entry.PathRules
                .Where(rule => rule.RuleType == PathRuleType.Disallow)
                .Select(x => new Uri(x.Path, UriKind.Relative)))
            .SelectMany(x => x)
            .Distinct()
            .Select(x => new Uri(new Uri(baseUri), x));
    }
}