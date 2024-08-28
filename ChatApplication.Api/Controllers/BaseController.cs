using ChatApplication.Business.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace ChatApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected ObjectResult BuildResponse(BaseResponse response)
    {
        return response.Success ? Ok(response) : StatusCode((int)response.ResponseCode, response);
    }
}