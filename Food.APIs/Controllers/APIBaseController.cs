using Food.APIs.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(ApiServerErrorResponse), StatusCodes.Status500InternalServerError)]
    public class APIBaseController : ControllerBase
    {
    }
}
