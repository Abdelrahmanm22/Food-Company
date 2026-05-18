using Food.APIs.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    [Route("errors/{code}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class EndPointNotFoundController : ControllerBase
    {
        public ActionResult Error(int code)
        {
            return NotFound(new ApiErrorResponse(code));
        }
    }
}
