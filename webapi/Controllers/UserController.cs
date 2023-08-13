using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Entities;
using webapi.Helper;
using webapi.Services;

namespace webapi.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        [Authorize]
        [Route("get/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetById(id);

            if (user==null) return NotFound();

            return Ok(user);
        }

        [HttpGet]
        [Authorize]
        [Route("get")]
        public async Task<IActionResult> GetConnectedUsers()
        {
            var userList = await _userService.GetConnectedUsers();

            if (userList==null) return NotFound();

            return Ok(userList);
        }
    }
}
