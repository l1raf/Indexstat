using Indexstat.RobotsParser.Models;

namespace Indexstat.RobotsParser;

public interface IRobotsParser
{
    public Task<RobotsFile> FromUriAsync(Uri robotsUri);
}