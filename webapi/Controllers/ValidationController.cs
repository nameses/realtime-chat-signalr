using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using webapi.Configuration;

namespace webapi.Controllers
{
    public class ValidationController : Controller
    {
        private readonly IOptions<JwtConfig> _settings;

        public ValidationController(IOptions<JwtConfig> settings)
        {
            _settings=settings;
        }
        [HttpGet]
        [Route("Validate")]
        public async Task<IActionResult> Validate(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenObject = handler.ReadToken(token) as JwtSecurityToken;

            if (token != null && token == _settings.Value.Key)
            {
                return Ok(true);
            }
            return Ok(false);
        }
    }
}
