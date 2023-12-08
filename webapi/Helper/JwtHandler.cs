using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace webapi.Helper
{
    public class JwtHandler : JwtBearerHandler
    {
        private readonly JwtValidator _jwtValidator;
        private readonly ILogger<JwtHandler> _logger;

        public JwtHandler(JwtValidator jwtValidator,
            ILogger<JwtHandler> logger,
            IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory loggerFact,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, loggerFact, encoder, clock)
        {
            _jwtValidator=jwtValidator;
            _logger=logger;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? token;

            if (ShouldSkip(Context)) return AuthenticateResult.NoResult();

            //if signalr hub -> get token from query parameters
            if (Context.Request.Path.StartsWithSegments("/chatsocket") ||
                Context.Request.Path.StartsWithSegments("/chatsocket/negotiate"))
            {
                if (!Context.Request.Query.TryGetValue("access_token", out var tokenValues))
                {
                    Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return AuthenticateResult.Fail("ID not found in query parameters.");
                }

                token = tokenValues.FirstOrDefault();
                if (string.IsNullOrEmpty(token))
                {
                    Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return AuthenticateResult.Fail("ID not found in query parameters.");
                }
            }
            //else -> get token from Authorization header
            else
            {
                if (!Context.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValues))
                {
                    Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return AuthenticateResult.Fail("Authorization header not found.");
                }

                var authorizationHeader = authorizationHeaderValues.FirstOrDefault();
                if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                {
                    return AuthenticateResult.Fail("Bearer token not found in Authorization header.");
                }

                token = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            var userId = _jwtValidator.Validate(token!);

            if (userId==null)
            {
                return AuthenticateResult.Fail("Token validation failed.");
            }

            var principal = GetClaims(token);

            return AuthenticateResult.Success(new AuthenticationTicket(principal, "JwtBearer"));
        }

        private static bool ShouldSkip(HttpContext context)
        {
            var excludedPaths = new[]
            {
                "/api/auth/register",
                "/api/auth/login"
            };
            var requestPath = context.Request.Path;
            return excludedPaths.Any(path => requestPath.StartsWithSegments(path));
        }

        private ClaimsPrincipal GetClaims(string Token)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(Token) as JwtSecurityToken;

            var claimsIdentity = new ClaimsIdentity(token?.Claims, "Token");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        }
    }
}
