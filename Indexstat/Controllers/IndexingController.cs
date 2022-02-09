using Indexstat.Enums;
using Indexstat.Services;
using Microsoft.AspNetCore.Mvc;

namespace Indexstat.Controllers;

public class IndexingController : ControllerBase
{
    private readonly IIndexingService _indexingService;

    public IndexingController(IIndexingService indexingService)
    {
        _indexingService = indexingService;
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