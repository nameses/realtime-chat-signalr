using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using System.Net;

namespace webapi.Helper
{
    public class JwtHandler : JwtBearerHandler
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JwtHandler> _logger;

        public JwtHandler(HttpClient httpClient,
            ILogger<JwtHandler> logger,
            IOptionsMonitor<JwtBearerOptions> options, 
            ILoggerFactory loggerFact,
            UrlEncoder encoder, 
            ISystemClock clock)
            : base(options, loggerFact, encoder, clock)
        {
            _httpClient = httpClient;
            _logger=logger;
        }
        private bool ShouldSkip(HttpContext context)
        {
            var excludedPaths = new[]
            {
                "/api/auth/register",
                "/api/auth/login",
                "/api/validate"
            };
            var requestPath = context.Request.Path;
            return excludedPaths.Any(path => requestPath.StartsWithSegments(path));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? token;

            if (ShouldSkip(Context)) return AuthenticateResult.NoResult();

            //get token from query parameters
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
            //get token from Authorization header
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

            // Call the API to validate the token
            var response = await _httpClient.GetAsync($"https://localhost:7161/api/validate?token={token}");

            _logger.LogInformation(response.ToString());
            // Return an authentication failure if the response is not successful
            if (!response.IsSuccessStatusCode)
            {
                return AuthenticateResult.Fail("Token validation failed.");
            }

            // Deserialize the response body to a custom object to get the validation result
            var validationResult = JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());

            // Return an authentication failure if the token is not valid
            if (!validationResult)
            {
                return AuthenticateResult.Fail("Token is not valid.");
            }

            // Set the authentication result with the claims from the API response
            var principal = GetClaims(token);

            return AuthenticateResult.Success(new AuthenticationTicket(principal, "CustomJwtBearer"));
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
