using Indexstat.DTOs;
using Indexstat.RobotsParser;
using Indexstat.RobotsParser.Models;

namespace Indexstat.Services;

public class RobotsService : IRobotsService
{
    private readonly IRobotsParser _robotsParser;

    public RobotsService(IRobotsParser robotsParser)
    {
        _robotsParser = robotsParser;
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
        //Yandex ignores user-agent * if yandex is found
        //Same for Google and googlebot
        if (robotsFile.SiteAccessEntries.Any(x => x.UserAgents.Contains(bot, StringComparer.InvariantCultureIgnoreCase))
            && robotsFile.SiteAccessEntries.Any(x => x.UserAgents.Contains("*")))
        {
            robotsFile.SiteAccessEntries = robotsFile.SiteAccessEntries
                .Where(x => x.UserAgents
                    .Contains(bot, StringComparer.InvariantCultureIgnoreCase));
        }

        return robotsFile.SiteAccessEntries
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