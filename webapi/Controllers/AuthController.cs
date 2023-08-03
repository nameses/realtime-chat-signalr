using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapi.Entities;
using webapi.Helper;
using webapi.Services;

namespace webapi.Controllers
{
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly JwtGenerator _jwtGenerator;

        public AuthController(IUserService userService,ILogger<AuthController> logger,JwtGenerator jwtGenerator)
        {
            _userService = userService;
            _logger=logger;
            _jwtGenerator=jwtGenerator;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] User userModel)
        {
            var createdId = await _userService.CreateAsync(userModel);

            if (createdId==null)
            {
                _logger.LogError($"User was not created");
            }

            return Ok(createdId);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var dbUser = await _userService.GetByUsername(user.Username);

            if (dbUser == null) 
            {
                return BadRequest("User not found");
            }
            if (!BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Password))
            {
                _logger.LogWarning($"User(Id={dbUser.Id}) password is not correct");
                return BadRequest("Password not correct.");
            }
            _logger.LogInformation($"User(Id={dbUser.Id}) password is correct");

            var token = _jwtGenerator.GenerateToken(user.Id);

            HttpContext.Response.Cookies.Append("auth_token", token);

            return Ok(new { Id=dbUser.Id,Username=dbUser.Username, Token=token});
        }
    }
}
