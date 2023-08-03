using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Text;
using webapi.Configuration;

namespace webapi.Controllers
{
    [Route("api")]
    public class ValidationController : Controller
    {
        private readonly IOptions<JwtConfig> _settings;
        private readonly ILogger<ValidationController> _logger;

        public ValidationController(IOptions<JwtConfig> settings, ILogger<ValidationController> logger)
        {
            _settings=settings;
            _logger=logger;
        }
        [HttpGet]
        [Route("validate")]
        public async Task<IActionResult> Validate(string token)
        {
            var secret = _settings.Value.Key;
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

            var issuer = _settings.Value.Issuer;
            var audience = _settings.Value.Audience;

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = securityKey
                }, out SecurityToken validatedToken);
            }
            catch
            {
                _logger.LogWarning("Validation of jwt token failed.");
                return BadRequest(false);
            }
            return Ok(true);
        }
    }
}
