namespace Indexstat.RobotsParser.Models;

public class SiteAccessEntry
{
    public IEnumerable<string> UserAgents { get; set; }
    
    public IEnumerable<SiteAccessPathRule> PathRules { get; set; }
}