using Indexstat.DTOs;

namespace Indexstat.Services;

public interface IRobotsService
{
    public Task<(string? error, RobotsTxtResponse? paths)> GetDisallowedUrls(Uri uri);
    
    public Task<(string? error, IEnumerable<Uri>? paths)> GetDisallowedToBotUrls(Uri uri, string bot);

    public Task<(string? error, RobotsHeadersResponse? headers)> GetRobotsResponseHeaders(Uri uri);
}