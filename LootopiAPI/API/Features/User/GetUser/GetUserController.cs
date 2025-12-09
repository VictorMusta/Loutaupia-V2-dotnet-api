using Microsoft.AspNetCore.Mvc;

namespace API.Features.User.GetUserUseCase
{
    [ApiController]
    [Route("api/user/get")]
    public class GetUserController : ControllerBase
    {
        //TODO: implement get user controller

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("GetUserController is working");

        }
    }
}
