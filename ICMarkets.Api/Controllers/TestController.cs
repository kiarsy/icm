using Microsoft.AspNetCore.Mvc;

namespace ICMarkets.Api.Controllers;

[ApiController]
[Route("api/test")]
public sealed class TestController : ControllerBase
{
    [HttpGet()]
    public IActionResult Test()
    {
        throw new Exception("Tet1");
    }

}