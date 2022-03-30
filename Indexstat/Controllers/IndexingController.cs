using Indexstat.DTOs;
using Indexstat.Enums;
using Indexstat.Services;
using Microsoft.AspNetCore.Mvc;

namespace Indexstat.Controllers;

[Route("[controller]")]
public class IndexingController : ControllerBase
{
    private static readonly Random Rand = new();
    private readonly IIndexingService _indexingService;

    public IndexingController(IIndexingService indexingService)
    {
        _indexingService = indexingService;
    }
    
    [HttpGet("source")]
    public async Task<ContentResult> GetSource(Uri uri, Uri cssFileAddress)
    {
        if (!uri.IsAbsoluteUri)
            return base.Content(string.Empty, "text/html");

        var (_, source) = await _indexingService.GetPageSource(uri, cssFileAddress);

        return base.Content(source ?? string.Empty, "text/html; charset=utf-8");
    }

    //TODO: remove
    [HttpGet("status/fake")]
    public async Task<ActionResult> GetIndexingStatusFake(Uri uri, SearchEngine engine)
    {
        switch (engine)
        {
            case SearchEngine.Google:
            {
                return Ok(new GoogleIndexingStatusResponse
                {
                    TotalSearchResults = Rand.Next(0, 1000)
                });
            }
            case SearchEngine.Yandex:
            {
                return Ok(new YandexIndexingStatusResponse
                {
                    TotalSearchResults = Rand.Next(0, 1000) + " results found"
                });
            }
            default:
            {
                return BadRequest("Wrong engine");
            }
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult> GetIndexingStatus(Uri uri, SearchEngine engine)
    {
        switch (engine)
        {
            case SearchEngine.Google:
            {
                var (error, response) = await _indexingService.GetGoogleIndexingStatus(uri);

                if (error is null)
                    return Ok(response);

                return BadRequest(error);
            }
            case SearchEngine.Yandex:
            {
                var (error, response) = await _indexingService.GetYandexIndexingStatus(uri);

                if (error is null)
                    return Ok(response);

                return BadRequest(error);
            }
            default:
            {
                return BadRequest("Wrong engine");
            }
        }
    }
}