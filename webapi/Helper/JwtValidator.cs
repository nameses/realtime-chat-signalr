using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using webapi.Configuration;

namespace webapi.Helper
{
    public class JwtValidator
    {

        private readonly IOptions<JwtConfig> _settings;
        private readonly ILogger<JwtValidator> _logger;

        public JwtValidator(IOptions<JwtConfig> settings, ILogger<JwtValidator> logger)
        {
            _settings=settings;
            _logger=logger;
        }

        public int? Validate(string token)
        {
            var secret = _settings.Value.Key;
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret!));

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
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                return userId;
            }
            catch (SecurityTokenExpiredException e)
            {
                _logger.LogWarning($"JWT token has expired: {e.Message}");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException e)
            {
                _logger.LogWarning($"JWT token signature validation failed: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred while validating JWT token: {e.Message}");
                return null;
            }
        }
    }
}
