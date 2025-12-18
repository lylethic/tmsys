using Microsoft.AspNetCore.Mvc;

namespace server.Controllers;

[Produces("application/json")]
[Route("/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthController : Controller
{
    [HttpGet]
    public IActionResult NotAuthorized()
    {
        return Unauthorized();
    }
}
