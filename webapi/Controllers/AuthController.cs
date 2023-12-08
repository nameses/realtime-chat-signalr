using Microsoft.AspNetCore.Mvc;
using webapi.Helper;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers
{
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly JwtGenerator _jwtGenerator;

        public AuthController(IUserService userService, ILogger<AuthController> logger, JwtGenerator jwtGenerator)
        {
            _userService = userService;
            _logger=logger;
            _jwtGenerator=jwtGenerator;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserModel user)
        {
            var userDb = await _userService.GetByUsername(user.Username);
            if (userDb != null)
                return BadRequest("Username already exists");

            var createdId = await _userService.CreateAsync(user);

            if (createdId==null)
            {
                _logger.LogError($"User was not created");
            }

            return Ok(createdId);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserModel user)
        {
            if (user==null || user.Password==null || user.Username==null)
                return BadRequest();
            var dbUser = await _userService.GetByUsername(user.Username);

            if (dbUser == null || dbUser.Username==null)
                return BadRequest("User not found");
            if (!BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Password))
                return BadRequest("Password not correct.");

            var token = _jwtGenerator.GenerateToken(dbUser.Id, dbUser.Username);

            HttpContext.Response.Cookies.Append("auth_token", token);

            return Ok(new { Id = dbUser.Id, Username = dbUser.Username, Token = token });
        }


        private class LoginResult
        {
            public int? Id;
            public string? Username;
            public string? Token;
        }
    }
}
