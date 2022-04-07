using Indexstat.RobotsParser.Models;

namespace Indexstat.RobotsParser;

public class RobotsParser : IRobotsParser
{
    private readonly HttpClient _httpClient;

    public RobotsParser(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RobotsFile> FromUriAsync(Uri robotsUri)
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
        _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        
        var response = await _httpClient.GetAsync(robotsUri);
        
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return new RobotsFile(robotsUri, await GetSiteAccessEntries(new StreamReader(stream)));
    }

    private async Task<IEnumerable<SiteAccessEntry>> GetSiteAccessEntries(StreamReader stream)
    {
        string? line;
        List<SiteAccessEntry> entries = new();
        List<string> userAgents = new();
        List<SiteAccessPathRule> rules = new();

        while ((line = await stream.ReadLineAsync()) != null)
        {
            line = line.Trim();
            line = RemovePossibleComment(line);

            var data = line.Split(":");

            if (data.Length != 2) continue;

            var field = data[0].Trim();
            var value = data[1].Trim();

            switch (field.ToLower())
            {
                case "user-agent":
                    if (rules.Count > 0)
                    {
                        entries.Add(new SiteAccessEntry
                        {
                            UserAgents = userAgents,
                            PathRules = rules
                        });

                        userAgents = new List<string>();
                        rules = new List<SiteAccessPathRule>();
                    }

                    userAgents.Add(value);
                    break;
                case "allow":
                    rules.Add(new SiteAccessPathRule
                    {
                        RuleType = PathRuleType.Allow,
                        Path = value
                    });
                    break;
                case "disallow":
                    rules.Add(new SiteAccessPathRule
                    {
                        RuleType = PathRuleType.Disallow,
                        Path = value
                    });
                    break;
            }
        }
        
        if (rules.Count > 0)
        {
            entries.Add(new SiteAccessEntry
            {
                UserAgents = userAgents,
                PathRules = rules
            });
        }

        return entries;
    }

    private string RemovePossibleComment(string line)
    {
        var index = line.IndexOf("#", StringComparison.Ordinal);

        if (index >= 0)
            line = line.Substring(0, index);

        return line;
    }
}