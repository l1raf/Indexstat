using Indexstat.Services;
using Microsoft.AspNetCore.Mvc;

namespace Indexstat.Controllers;

[Route("[controller]")]
public class RobotsController : ControllerBase
{
    private readonly IRobotsService _robotsService;

    public RobotsController(IRobotsService robotsService)
    {
        _robotsService = robotsService;
    }
    
    [HttpGet("disallowed")]
    public async Task<ActionResult> GetDisallowedUrls(Uri uri)
    {
        var result = await _robotsService.GetDisallowedUrls(uri);

        if (result.error is null)
            return Ok(result.paths);

        return BadRequest(result.error);
    }

    [HttpGet("disallowed/google")]
    public async Task<ActionResult> GetDisallowedToGoogleUrls(Uri uri)
    {
        var result = await _robotsService.GetDisallowedToBotUrls(uri, "googlebot");

        if (result.error is null)
            return Ok(result.paths);

        return BadRequest(result.error);
    }

    [HttpGet("disallowed/yandex")]
    public async Task<ActionResult> GetDisallowedToYandexUrls(Uri uri)
    {
        var result = await _robotsService.GetDisallowedToBotUrls(uri, "yandex");

        if (result.error is null)
            return Ok(result.paths);

        return BadRequest(result.error);
    }
}