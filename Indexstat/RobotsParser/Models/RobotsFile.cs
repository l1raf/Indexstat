namespace Indexstat.RobotsParser.Models;

public class RobotsFile
{
    public Uri Uri { get; }
    
    public IEnumerable<SiteAccessEntry> SiteAccessEntries { get; set; }
    
    public RobotsFile(Uri uri, IEnumerable<SiteAccessEntry> siteAccessEntries)
    {
        Uri = uri;
        SiteAccessEntries = siteAccessEntries;
    }
}