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
                return Ok(false);
            }
            return Ok(true);
        }
    }
}
